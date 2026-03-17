# Features Guide

A practical guide to every feature in JV.ResultUtilities, with real-world examples and guidance on when to use what.

## Table of Contents

- [Result Creation](#result-creation)
- [Accessing Values Safely](#accessing-values-safely)
- [Functional Combinators](#functional-combinators)
  - [Map](#map)
  - [Bind](#bind)
  - [Match](#match)
  - [Ensure](#ensure)
  - [Do / DoAsync](#do--doasync)
- [Merging Results](#merging-results)
- [Collection Operations](#collection-operations)
  - [TraverseAll](#traverseall)
  - [TraversePartialWithErrors](#traversepartialwitherrors)
  - [Choosing the Right Traverse](#choosing-the-right-traverse)
- [Casting Failed Results](#casting-failed-results)
- [Implicit Conversions](#implicit-conversions)
- [Validation Messages](#validation-messages)
  - [Defining Validation Keys](#defining-validation-keys)
  - [Parameter Passing](#parameter-passing)
  - [Field Names](#field-names)
  - [Lenient Creation](#lenient-creation)
  - [Raw Parameters](#raw-parameters)
- [Validation Pipeline](#validation-pipeline)
  - [Sync Rules](#sync-rules)
  - [Async Rules](#async-rules)
  - [Short-Circuit Mode](#short-circuit-mode)
- [Exception Bridging](#exception-bridging)
- [Memoization](#memoization)
  - [Simple Memoization](#simple-memoization)
  - [Async Memoization](#async-memoization)
  - [Expiring Cache with Size Limits](#expiring-cache-with-size-limits)
  - [Result Memoization](#result-memoization)
- [Async Pipelines](#async-pipelines)

---

## Result Creation

`Result` (no value) and `Result<T>` (with value) are the two result types. Create them with static factory methods.

```c#
// Success — no value
Result ok = Result.Ok();

// Success — with value
Result<User> user = Result.Ok(new User("Alice", "alice@example.com"));

// Failure — from a validation key
var NotFoundKey = ValidationKeyDefinition.Create("user.not.found")
    .WithStringParameter("userId");

Result error = Result.Error(NotFoundKey, "user-123");

// Failure — typed, for when your return type is Result<T>
Result<User> typedError = Result.Error<User>(NotFoundKey, "user-123");

// Failure — from an existing validation message
var message = ValidationMessage.Create(NotFoundKey, "user-123");
Result<User> fromMessage = Result.Error(message);

// Failure — from multiple messages
Result<User> multiError = Result.Error(new[] { message1, message2 });
```

**When to use what:**
- `Result.Ok()` / `Result.Ok(value)` for successful outcomes
- `Result.Error(key, params)` for non-generic results (e.g., `Result ValidateSomething()`)
- `Result.Error<T>(key, params)` for typed results (e.g., `Result<User> GetUser()`)
- `Result.Error(validationMessages)` when you've accumulated a list of errors

---

## Accessing Values Safely

`Result<T>.Value` throws `InvalidOperationException` if the result is a failure. Always check first or use combinators.

```c#
Result<Order> orderResult = GetOrder(orderId);

// Option 1: Check first
if (orderResult.IsSuccessful)
{
    var order = orderResult.Value;
    ProcessOrder(order);
}

// Option 2: Use Match (preferred — forces you to handle both cases)
var response = orderResult.Match(
    onSuccess: order => Ok(MapToDto(order)),
    onFailure: errors => BadRequest(errors)
);

// Option 3: Use Map/Bind to chain operations (value is never accessed directly)
var dto = orderResult
    .Map(order => MapToDto(order))
    .Match(
        onSuccess: dto => Ok(dto),
        onFailure: errors => NotFound(errors)
    );
```

**When to use what:**
- `Match` when you need to produce a final value (e.g., an HTTP response) from either case
- `Map`/`Bind` chains when you're transforming data through multiple steps
- `IsSuccessful` + `.Value` only in performance-critical code where you control both sides

---

## Functional Combinators

### Map

Transforms the value inside a successful result. If the result is a failure, the transformation is skipped and errors are forwarded.

```c#
// Scenario: convert a domain entity to a DTO
Result<UserDto> dto = GetUser(userId)
    .Map(user => new UserDto(user.Name, user.Email));

// Async variant — when the transformation itself is async
Result<byte[]> avatar = await GetUser(userId)
    .MapAsync(async user => await blobStore.DownloadAsync(user.AvatarPath));

// MapSync — when you have a Task<Result<T>> but a synchronous mapper
Result<string> name = await GetUserAsync(userId)
    .MapSync(user => user.Name);
```

**Use Map when:** the transformation always succeeds (it cannot fail on its own).

### Bind

Chains an operation that itself returns a `Result<T>`. Unlike Map, the chained operation can fail.

```c#
// Scenario: look up a user, then load their subscription (which might not exist)
Result<Subscription> sub = GetUser(userId)
    .Bind(user => GetSubscription(user.SubscriptionId));

// Async variant
Result<Invoice> invoice = await GetUserAsync(userId)
    .BindAsync(async user => await billingService.GetLatestInvoiceAsync(user.Id));

// BindSync — Task<Result<T>> + synchronous binder
Result<Address> address = await GetUserAsync(userId)
    .BindSync(user => ValidateAddress(user.Address));
```

**Use Bind when:** the next step can also fail (returns a `Result`).

### Match

Pattern-matches on success/failure to produce a final value. This is typically your "exit ramp" from the Result world.

```c#
// Scenario: ASP.NET controller returning an IActionResult
[HttpGet("{id}")]
public async Task<IActionResult> GetOrder(int id)
{
    return await orderService.FindAsync(id)
        .MatchAsync(
            onSuccess: order => Ok(order),
            onFailure: errors => NotFound(new { errors = errors.Select(e => e.TranslationKey) })
        );
}
```

**Use Match when:** you need to leave the Result monad and produce a concrete value for both cases.

### Ensure

Validates a condition on the value. If the predicate returns false, the result becomes a failure.

```c#
var AgeMinimumKey = ValidationKeyDefinition.Create("user.age.minimum")
    .WithIntParameter("age")
    .WithIntParameter("minimum");

Result<User> validatedUser = GetUser(userId)
    .Ensure(user => user.Age >= 18, AgeMinimumKey, user.Age, 18);
```

**Use Ensure when:** you need to add a validation check in the middle of a chain.

### Do / DoAsync

Execute a side-effect on a successful result without changing the value. The result passes through unchanged.

```c#
// Scenario: log an audit event after a successful operation
Result<Order> order = CreateOrder(request)
    .Do(o => logger.LogInformation("Order {OrderId} created", o.Id));

// Async side-effect on a Task<Result<T>>
Result<Order> order = await CreateOrderAsync(request)
    .DoAsync(async o => await eventBus.PublishAsync(new OrderCreatedEvent(o.Id)));
```

**Use Do/DoAsync when:** you need logging, event publishing, metrics, or other side-effects that shouldn't change the result.

---

## Merging Results

Combine multiple independent results into one.

```c#
// Scenario: validate several fields independently, then combine
Result nameResult = ValidateName(request.Name);
Result emailResult = ValidateEmail(request.Email);
Result ageResult = ValidateAge(request.Age);

// Merge non-generic results
Result combined = new[] { nameResult, emailResult, ageResult }.MergeResults();

if (combined.IsFailure)
    return BadRequest(combined.ValidationMessages); // all errors at once

// Merge typed results
Result<Name> name = ParseName(input);
Result<Email> email = ParseEmail(input);

Result<IEnumerable<object>> merged = new[] { name.Map(n => (object)n), email.Map(e => (object)e) }
    .MergeResults();

// Merge a typed result with non-generic results
Result<User> user = GetUser(userId)
    .MergeResults(new[] { ValidatePermissions(userId), ValidateQuota(userId) });
```

**Use MergeResults when:** you have independent validations or operations that should all run and you want a combined error report.

---

## Collection Operations

### TraverseAll

Transforms every item in a collection. If **any** item fails, the entire result is a failure containing all errors.

```c#
// Scenario: validate and import a batch of records — all must succeed
var importResults = await records.TraverseAllAsync(async record =>
{
    var validated = ValidateRecord(record);
    if (validated.IsFailure) return validated.Cast<ImportedRecord>();

    return await importService.ImportAsync(record);
});

if (importResults.IsFailure)
{
    // None were imported — show all validation errors
    return BadRequest(importResults.ValidationMessages);
}

var imported = importResults.Value; // all records

// Parallel variant — for I/O-bound transforms where order doesn't matter
var results = await urls.TraverseAllParallelAsync(async url =>
    await httpClient.FetchAsync(url));
```

**Use TraverseAll when:** it's all-or-nothing — either every item succeeds or the whole batch fails.

### TraversePartialWithErrors

Transforms a collection, keeping successful results and collecting errors separately. The operation always "succeeds" — you get the good results plus a list of what went wrong.

```c#
// Scenario: send notifications to a list of users — some might fail, but don't block the rest
var (sent, errors) = users.TraversePartialWithErrors(user =>
{
    if (string.IsNullOrEmpty(user.Email))
        return Result.Error<NotificationResult>(EmailMissingKey, user.Id);

    return Result.Ok(new NotificationResult(user.Id, "queued"));
});

logger.LogInformation("Sent {Count} notifications", sent.Value.Count());
if (errors.Any())
    logger.LogWarning("Failed for {Count} users: {Errors}", errors.Count(), errors);

// Async sequential variant
var (results, errors) = await items.TraversePartialWithErrorsAsync(async item =>
    await ProcessItemAsync(item));

// Async parallel variant — for independent I/O-bound operations
var (results, errors) = await items.TraversePartialWithErrorsParallelAsync(async item =>
    await externalService.CallAsync(item));
```

**Use TraversePartialWithErrors when:** you want to process as many items as possible and report failures separately (e.g., batch notifications, data migration).

### Choosing the Right Traverse

| Scenario | Method | Behavior |
|----------|--------|----------|
| All must succeed (e.g., transaction) | `TraverseAll` | Fails if any item fails |
| All must succeed, async sequential | `TraverseAllAsync` | Same, awaits one by one |
| All must succeed, async parallel | `TraverseAllParallelAsync` | Same, runs all concurrently |
| Best-effort (process what you can) | `TraversePartialWithErrors` | Returns successes + error list |
| Best-effort, async sequential | `TraversePartialWithErrorsAsync` | Same, awaits one by one |
| Best-effort, async parallel | `TraversePartialWithErrorsParallelAsync` | Same, runs all concurrently |

**Sequential vs Parallel:** Use sequential when operations depend on shared state or you need to respect rate limits. Use parallel for independent I/O-bound operations (API calls, database reads).

---

## Casting Failed Results

`Cast<TResult>()` converts a failed result to a different type, forwarding all validation messages. Only works on failures.

```c#
// Scenario: a shared validation method returns Result<string>, but you need Result<User>
Result<string> validationResult = ValidateInput(request);

if (validationResult.IsFailure)
    return validationResult.Cast<User>(); // forward errors as Result<User>

// Continue with the validated input
return CreateUser(validationResult.Value);
```

**Use Cast when:** you need to re-type a failed result in a method that returns a different `Result<T>`. For successful results, use `Map` instead.

---

## Implicit Conversions

The library provides implicit operators for concise code:

```c#
// Return a value directly from a Result<T>-returning method
public Result<User> GetDefaultUser()
{
    return new User("Guest", "guest@example.com"); // implicitly wraps in Result.Ok
}

// Return a validation message as a failure
public Result<User> GetUser(string id)
{
    if (string.IsNullOrEmpty(id))
        return ValidationMessage.Create(IdRequiredKey); // implicitly wraps in Result.Error

    return userRepository.Find(id);
}

// Convert a non-generic failure to Result<T> (only works for failures)
public Result<User> CreateUser(UserRequest request)
{
    Result validation = ValidateRequest(request);
    if (validation.IsFailure)
        return validation; // implicit conversion — carries errors over

    return Result.Ok(new User(request.Name, request.Email));
}
```

**Gotcha:** Converting a successful `Result` (non-generic) to `Result<T>` throws `NotSupportedException` — there's no value to carry over. This only works for failures.

---

## Validation Messages

### Defining Validation Keys

`ValidationKeyDefinition` defines type-safe validation message templates with strongly-typed parameters.

```c#
// Simple key — no parameters
private static readonly ValidationKeyDefinition NameRequiredKey =
    ValidationKeyDefinition.Create("user.name.required");

// With typed parameters
private static readonly ValidationKeyDefinition AgeTooLowKey =
    ValidationKeyDefinition.Create("user.age.too.low")
    .WithIntParameter("age")
    .WithIntParameter("minimum", 18); // 18 is the default value

// Separate key and translation key (for i18n systems)
private static readonly ValidationKeyDefinition EmailInvalidKey =
    ValidationKeyDefinition.Create("user.email.invalid", "The email address '{email}' is not valid.")
    .WithEmailParameter("email");

// Supported parameter types:
// .WithStringParameter("name")
// .WithIntParameter("count")
// .WithDecimalParameter("amount")
// .WithBooleanParameter("isActive")
// .WithDateTimeParameter("createdAt")
// .WithEmailParameter("email")     — validates email format
// .WithUriParameter("website")     — validates URI format
// .WithGuidParameter("id")
```

### Parameter Passing

Multiple ways to supply parameters when creating messages or errors:

```c#
var OrderErrorKey = ValidationKeyDefinition.Create("order.error")
    .WithStringParameter("orderId")
    .WithDecimalParameter("amount")
    .WithStringParameter("reason", "Unknown"); // default value

// 1. Positional array — matched by order
Result.Error(OrderErrorKey, new object[] { "ORD-123", 99.99m, "Insufficient funds" });

// 2. Anonymous object — matched by name (order doesn't matter)
Result.Error(OrderErrorKey, new { reason = "Insufficient funds", orderId = "ORD-123", amount = 99.99m });

// 3. Dictionary — for dynamic parameters
var parameters = new Dictionary<string, object>
{
    ["orderId"] = "ORD-123",
    ["amount"] = 99.99m
    // "reason" uses its default value "Unknown"
};
Result.Error(OrderErrorKey, parameters);

// 4. Single value — when only one parameter has no default
Result.Error(EmailInvalidKey, "not-an-email"); // maps to "email" parameter

// 5. No parameters — when all have defaults
var AllDefaultsKey = ValidationKeyDefinition.Create("all.defaults")
    .WithStringParameter("env", "production")
    .WithIntParameter("retries", 3);
ValidationMessage.Create(AllDefaultsKey); // uses all defaults
```

### Field Names

Associate a validation key with a form field for easier error mapping in UIs:

```c#
private static readonly ValidationKeyDefinition EmailRequiredKey =
    ValidationKeyDefinition.Create("user.email.required")
    .WithFieldName("email");

// In your frontend, you can use FieldName to display errors next to the right input
var message = ValidationMessage.Create(EmailRequiredKey);
var fieldName = message.KeyDefinition.FieldName; // "email"
```

**Use FieldName when:** your API returns validation errors that a frontend needs to map to specific form fields.

### Lenient Creation

`CreateLenient` bypasses parameter type validation. Parameters are simply converted via `ToString()`.

```c#
// Scenario: bridging with external validation systems that provide arbitrary error data
var externalError = externalService.Validate(input);
var message = ValidationMessage.CreateLenient(GenericErrorKey, externalError.Code, externalError.Detail);

// Scenario: when parameter types don't perfectly match the definition
var message = ValidationMessage.CreateLenient(AmountErrorKey, someUntypedValue);
```

**Use CreateLenient when:** you're integrating with external systems or working with dynamic data where strict type validation would be impractical. Prefer `Create` for your own code — it catches type mismatches at runtime.

### Raw Parameters

`RawParameters` gives you the original typed values before string formatting:

```c#
var AmountKey = ValidationKeyDefinition.Create("payment.amount.exceeded")
    .WithDecimalParameter("amount")
    .WithDecimalParameter("limit");

var message = ValidationMessage.Create(AmountKey, new object[] { 150.00m, 100.00m });

message.Parameters;    // ["150.00", "100.00"] — formatted strings
message.RawParameters; // [150.00m, 100.00m]  — original decimal values

// Scenario: programmatically inspect error parameters for retry logic
if (message.RawParameters is [decimal attempted, decimal limit])
{
    logger.LogWarning("Payment of {Amount} exceeded limit of {Limit}", attempted, limit);
}
```

**Use RawParameters when:** you need to programmatically inspect or transform parameter values (not just display them).

---

## Validation Pipeline

### Sync Rules

Build a pipeline of validation rules and execute them all against an object:

```c#
// Scenario: validate a user registration request
var NameTooShortKey = ValidationKeyDefinition.Create("user.name.too.short")
    .WithIntParameter("length")
    .WithIntParameter("minimum");

var EmailInvalidKey = ValidationKeyDefinition.Create("user.email.invalid")
    .WithStringParameter("email");

var pipeline = new ValidationPipeline<RegistrationRequest>()
    .AddRule(req => req.Name.Length >= 3
        ? Result.Ok()
        : Result.Error(NameTooShortKey, new object[] { req.Name.Length, 3 }))
    .AddRule(req => IsValidEmail(req.Email)
        ? Result.Ok()
        : Result.Error(EmailInvalidKey, req.Email));

Result<RegistrationRequest> result = pipeline.Validate(request);

if (result.IsSuccessful)
{
    // All rules passed — result.Value is the validated request
    var user = CreateUser(result.Value);
}
else
{
    // result.ValidationMessages contains ALL failed rules
    return BadRequest(result.ValidationMessages);
}
```

### Async Rules

Mix sync and async rules in the same pipeline. Async rules run concurrently by default.

```c#
// Scenario: validate against database and external service
var pipeline = new ValidationPipeline<RegistrationRequest>()
    .AddRule(req => req.Name.Length >= 3           // sync — fast checks first
        ? Result.Ok()
        : Result.Error(NameTooShortKey, new object[] { req.Name.Length, 3 }))
    .AddRule(async req =>                          // async — database check
    {
        var exists = await db.Users.AnyAsync(u => u.Email == req.Email);
        return exists
            ? Result.Error(EmailTakenKey, req.Email)
            : Result.Ok();
    })
    .AddRule(async req =>                          // async — external service
    {
        var isDisposable = await emailChecker.IsDisposableAsync(req.Email);
        return isDisposable
            ? Result.Error(DisposableEmailKey, req.Email)
            : Result.Ok();
    });

// Must use ValidateAsync when async rules are present
Result<RegistrationRequest> result = await pipeline.ValidateAsync(request);
```

**Gotcha:** Calling `Validate()` (sync) when async rules are registered throws `InvalidOperationException`. Use `ValidateAsync()`.

### Short-Circuit Mode

Stop validation on the first failure instead of collecting all errors:

```c#
// Scenario: expensive validations — don't run the rest if basic checks fail
var pipeline = new ValidationPipeline<PaymentRequest> { ShortCircuit = true }
    .AddRule(req => req.Amount > 0                 // cheap check
        ? Result.Ok()
        : Result.Error(InvalidAmountKey, req.Amount))
    .AddRule(req =>                                // medium cost
    {
        var account = GetAccount(req.AccountId);
        return account.Balance >= req.Amount
            ? Result.Ok()
            : Result.Error(InsufficientFundsKey, new object[] { req.Amount, account.Balance });
    })
    .AddRule(async req =>                          // expensive — external fraud check
    {
        var score = await fraudService.ScoreAsync(req);
        return score < 0.8
            ? Result.Ok()
            : Result.Error(FraudSuspectedKey, score);
    });

// If Amount <= 0, the balance check and fraud check are never executed
var result = await pipeline.ValidateAsync(request);
```

**Use ShortCircuit when:** later rules are expensive, or earlier failures make later checks meaningless (e.g., don't check uniqueness if the format is invalid).

---

## Exception Bridging

Bridge between Result-based code and exception-based code:

```c#
// Throw if a result is a failure
Result<User> user = GetUser(userId);
user.ThrowIfFailure(); // throws ResultException<User> with all validation messages

// ResultException sets Exception.Message with a summary of all errors
try
{
    GetUser(userId).ThrowIfFailure();
}
catch (ResultException<User> ex)
{
    // ex.Message contains: "ValidationKey: user.not.found Parameters: user-123"
    // ex.ValidationMessages contains the original validation messages
    logger.LogError(ex, "Failed to get user");
}

// Construct exceptions directly
var exception = new ResultException(result);           // from non-generic Result
var exception = new ResultException<User>(userResult); // from Result<User>
var exception = new ResultException(validationMessage);// from a single message
var exception = new ResultException(validationKey);    // from a key definition
```

**Use exception bridging when:** you need to interop with code that expects exceptions (middleware, third-party libraries, top-level error handlers). Prefer staying in the Result world for your own code.

---

## Memoization

### Simple Memoization

Cache function results to avoid repeated computation:

```c#
// Scenario: expensive calculation called many times with the same input
Func<string, decimal> calculateDiscount = customerId =>
{
    // Hits database, runs business rules...
    return discountEngine.Calculate(customerId);
};

var memoized = calculateDiscount.Memoize();

var discount1 = memoized("CUST-001"); // computes and caches
var discount2 = memoized("CUST-001"); // instant — returns cached value
var discount3 = memoized("CUST-002"); // computes and caches (different key)

// Multi-parameter memoization (up to 3 parameters)
Func<string, int, decimal> calculateShipping = (region, weight) =>
    shippingService.GetRate(region, weight);

var memoizedShipping = calculateShipping.Memoize();
```

**Use Memoize when:** you have a pure function (same input = same output) that is called repeatedly with the same arguments.

### Async Memoization

Cache async function results with per-key locking to prevent thundering herd:

```c#
// Scenario: external API calls that are expensive and return the same data for the same input
Func<string, Task<ExchangeRate>> getRate = async currency =>
    await rateApi.GetCurrentRateAsync(currency);

var memoizedRate = getRate.MemoizeAsync();

// First call for "USD" — makes the API call, caches result
var usdRate = await memoizedRate("USD");

// Second call for "USD" — returns cached result immediately
var usdRateAgain = await memoizedRate("USD");

// Concurrent calls for the same key — only one API call is made (per-key lock)
var tasks = Enumerable.Range(0, 100).Select(_ => memoizedRate("EUR"));
var results = await Task.WhenAll(tasks); // only 1 actual API call

// For statistics and cache management, use the factory directly
var memoized = MemoizationFactory.CreateMemoizedAsync<string, ExchangeRate>(getRate);
var rate = await memoized.InvokeAsync("GBP");

Console.WriteLine($"Cache hits: {memoized.HitCount}");
Console.WriteLine($"Cache misses: {memoized.MissCount}");
Console.WriteLine($"Hit ratio: {memoized.HitRatio:F1}%");
```

**Use MemoizeAsync when:** you have async I/O-bound functions (API calls, database queries) that return deterministic results for the same input.

### Expiring Cache with Size Limits

For long-running applications where cached data can become stale:

```c#
// Scenario: cache product prices that change periodically
var memoizedPrice = MemoizationFactory.CreateConfigurable<string, decimal>(
    productId => productService.GetPrice(productId),
    maxCacheSize: 1000,                     // evict least-recently-used after 1000 entries
    expiration: TimeSpan.FromMinutes(15)    // re-fetch after 15 minutes
);

var price = memoizedPrice.Invoke("PROD-001");

// Monitor cache health
if (memoizedPrice.HitRatio < 50)
    logger.LogWarning("Cache hit ratio is low ({Ratio}%), consider increasing cache size",
        memoizedPrice.HitRatio);
```

**Use CreateConfigurable when:** data can go stale, memory is a concern, or you're running in a long-lived process (web server, background service).

### Result Memoization

Memoize functions that return `Result<T>` — both successes and failures are cached:

```c#
// Scenario: expensive validation that gets called repeatedly for the same entity
Func<string, Result<ValidatedEmail>> validateEmail = email =>
{
    // DNS lookup, format check, disposable email check...
    return emailValidator.Validate(email);
};

var memoizedValidation = validateEmail.MemoizeResult();
var result1 = memoizedValidation("alice@example.com"); // validates and caches
var result2 = memoizedValidation("alice@example.com"); // cached

// With custom key selector — useful when the input is complex but the cache key is simple
Func<UserRequest, Result<User>> processUser = req => /* ... */;

var memoized = processUser.MemoizeResultWithKey(req => req.Email);
// Different UserRequest objects with the same Email share the cached result

// With statistics
var configurable = validateEmail.CreateMemoizedResult();
var result = configurable.Invoke("bob@example.com");
Console.WriteLine($"Validation cache hit ratio: {configurable.HitRatio:F1}%");
```

**Use MemoizeResult when:** you have expensive Result-returning functions (validation, lookups) called repeatedly with the same inputs.

---

## Async Pipelines

Combine all features into async pipelines for real-world service methods:

```c#
// Scenario: full order processing pipeline
public async Task<IActionResult> PlaceOrder([FromBody] OrderRequest request)
{
    // Step 1: Validate the request
    var pipeline = new ValidationPipeline<OrderRequest> { ShortCircuit = true }
        .AddRule(req => req.Items.Any()
            ? Result.Ok()
            : Result.Error(EmptyCartKey))
        .AddRule(async req =>
        {
            var inStock = await inventory.CheckAllAsync(req.Items);
            return inStock ? Result.Ok() : Result.Error(OutOfStockKey);
        });

    var validationResult = await pipeline.ValidateAsync(request);

    // Step 2: Process through functional chain
    return await validationResult
        .MapAsync(async req => await pricingService.CalculateTotalAsync(req))
        .BindAsync(async total => await paymentService.ChargeAsync(request.PaymentMethod, total))
        .DoAsync(async charge => await eventBus.PublishAsync(new PaymentProcessedEvent(charge.Id)))
        .BindAsync(async charge => await orderService.CreateAsync(request, charge))
        .DoAsync(async order => await emailService.SendConfirmationAsync(order))
        .MatchAsync(
            onSuccess: order => Ok(new { orderId = order.Id, status = "confirmed" }),
            onFailure: errors => BadRequest(new { errors = errors.Select(e => e.MapToErrorMessage()) })
        );
}

// Scenario: batch import with partial success reporting
public async Task<IActionResult> ImportProducts([FromBody] ProductImportRequest[] products)
{
    var (imported, errors) = await products.TraversePartialWithErrorsAsync(async product =>
    {
        var validated = validateProduct.Validate(product);
        if (validated.IsFailure)
            return validated.Cast<ImportedProduct>();

        return await importService.ImportAsync(validated.Value);
    });

    return Ok(new
    {
        imported = imported.Value.Count(),
        failed = errors.Count(),
        errors = errors.Select(e => e.MapToErrorMessage())
    });
}
```
