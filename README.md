
# JV.Utils - Result Type for Error Handling

## Overview

This package provides a robust Result type implementation for handling success and failure states in your application. It includes type-safe validation messages using `TranslationKeyDefinition` to ensure that users provide the correct number and types of parameters when creating validation messages.

Results are binary - they are either successful (contain no validation messages) or unsuccessful (contain one or more
validation messages).

## Usage Examples

### Creating a TranslationKeyDefinition

```csharp
// Create a simple key without parameters
var simpleKey = TranslationKeyDefinition.Create("user.invalid", "user.invalid");

// Create a key with specific parameters
var nameKey = TranslationKeyDefinition.Create(
    "user.name.invalid", 
    "user.name.invalid",
    new TranslationParameter("name", ParameterType.String),
    new TranslationParameter("minLength", ParameterType.Integer)
);

// Or use fluent extensions
using JV.Utils.Extensions;

var priceKey = TranslationKeyDefinition
    .Create("product.price.invalid", "product.price.invalid")
    .WithStringParameter("product")
    .WithDecimalParameter("price")
    .WithDecimalParameter("minimumPrice");

    // Example using new parameter types
    var appointmentKey = TranslationKeyDefinition
    .Create("appointment.invalid", "appointment.invalid")
    .WithStringParameter("patientName")
    .WithDateOnlyParameter("appointmentDate")
    .WithTimeOnlyParameter("appointmentTime")
    .WithTimeSpanParameter("duration");

    var userKey = TranslationKeyDefinition
    .Create("user.registration", "user.registration")
    .WithStringParameter("username")
    .WithEmailParameter("email")
    .WithPhoneNumberParameter("phone")
    .WithBooleanParameter("isActive")
    .WithGuidParameter("userId");
```

### Creating ValidationMessages

```csharp
// Create a validation message with type safety
var errorMessage = ValidationMessage.CreateError(nameKey, "John", 3);
var validationMessage = ValidationMessage.CreateError(nameKey, "John", 3);

// This would throw an exception at runtime since the parameters don't match
// ValidationMessage.CreateError(nameKey, 3, "John"); // Invalid parameter types
// ValidationMessage.CreateError(nameKey, "John"); // Missing parameter
```

## Result Success/Failure Logic

A result is considered:

- **Successful** when it contains no validation messages
- **Unsuccessful** when it contains one or more validation messages

```csharp
// Successful result (no validation messages)
var successResult = Result.Ok();

// Unsuccessful result (contains validation messages)
var errorResult = Result.Error(translationKey, "Some parameter");

// Working with typed results
var userResult = Result.Ok(user); // Successful with value
```

## Best Practices

1. Define translation keys in a central location for reuse
2. Use meaningful parameter names that match your translation system
3. Consider creating constants for commonly used translation keys
4. Take advantage of the type safety to catch parameter errors early
5. Use `Result.Ok()` for successful operations without return values
6. Use `Result.Ok(value)` for successful operations with return values
7. Use `Result.Error()` to indicate validation failures

## Working with Results

### Basic Usage

```csharp
// Creating results
var successResult = Result.Ok();
var errorResult = Result.Error("error.key", "Some parameter");

// Working with typed results
var user = new User { Name = "John" };
var successWithValue = Result.Ok(user);

// Checking result status
if (result.IsSuccessful)
{
    // Process successful result
}
else
{
    // Handle failure, access validation messages
    foreach (var message in result.ValidationMessages)
    {
        Console.WriteLine(message.MapToErrorMessage());
    }
}
```

### Implicit Conversions

The Result type supports implicit conversions for cleaner, more natural code:

```csharp
// Traditional approach (still works)
public Result<User> GetUser(int id)
{
    if (id <= 0)
        return Result.Error<User>(invalidIdKey, id);
    
    var user = _repository.GetById(id);
    return Result.Ok(user);
}

// With implicit conversions - much cleaner!
public Result<User> GetUser(int id)
{
    if (id <= 0)
        return ValidationMessage.CreateError(invalidIdKey, id); // Direct error return
    
    var user = _repository.GetById(id);
    return user; // Direct value return
}

// Multiple errors with implicit conversion
public Result<Account> CreateAccount(CreateAccountRequest request)
{
    var errors = new List<ValidationMessage>();
    
    if (string.IsNullOrEmpty(request.Username))
        errors.Add(ValidationMessage.CreateError(usernameRequiredKey));
        
    if (string.IsNullOrEmpty(request.Email))
        errors.Add(ValidationMessage.CreateError(emailRequiredKey));
        
    if (errors.Any())
        return errors.ToArray(); // Direct return of error array
        
    var account = new Account(request);
    return account; // Direct return of value
}

// API Controller benefits
[HttpGet("{id}")]
public async Task<Result<ProductDto>> GetProduct(int id)
{
    if (id <= 0)
        return ValidationMessage.CreateError(invalidIdKey, id);
        
    var product = await _productService.GetByIdAsync(id);
    if (product == null)
        return ValidationMessage.CreateError(productNotFoundKey, id);
        
    var dto = _mapper.Map<ProductDto>(product);
    return dto; // Clean return
}
```

### Exception-Safe Operations

Handle exceptions gracefully with automatic Result wrapping:

```csharp
// Sync operations with exception handling
public Result<User> ParseUserFromJson(string json)
{
    return Result.Try(
        () => JsonSerializer.Deserialize<User>(json), 
        parseErrorKey, 
        "JSON"
    );
}

// Async operations with exception handling
public async Task<Result<User>> FetchUserFromApiAsync(int userId)
{
    return await Result.TryAsync(
        async () => await _httpClient.GetFromJsonAsync<User>($"users/{userId}"),
        apiErrorKey,
        userId
    );
}

// Database operations
public Result<User> SaveUser(User user)
{
    return Result.Try(
        () => {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        },
        saveErrorKey,
        user.Id
    );
}

// File operations
public async Task<Result<string>> ReadFileAsync(string path)
{
    return await Result.TryAsync(
        () => File.ReadAllTextAsync(path),
        fileReadErrorKey,
        path
    );
}
```

### Collection Operations

Work with collections of Results efficiently:

```csharp
// Transform all items - fails if any transformation fails
public Result<IEnumerable<UserDto>> GetAllUserDtos(IEnumerable<int> userIds)
{
    return userIds.TraverseAll(id => GetUser(id).Map(user => new UserDto(user)));
}

// Transform keeping only successful results
public Result<IEnumerable<UserDto>> GetValidUserDtos(IEnumerable<int> userIds)
{
    return userIds.TraversePartial(id => GetUser(id).Map(user => new UserDto(user)));
}

// Practical example: Bulk user validation
public Result<IEnumerable<User>> ValidateUsers(IEnumerable<CreateUserRequest> requests)
{
    return requests.TraverseAll(request => 
    {
        return ValidateUserRequest(request)
            .Map(validRequest => new User(validRequest));
    });
}

// Processing orders with partial success
public async Task<Result<IEnumerable<ProcessedOrder>>> ProcessOrdersAsync(IEnumerable<Order> orders)
{
    return await orders.TraverseAllAsync(async order => 
        await ProcessSingleOrderAsync(order));
}
```

### Conditional Operations

Add validation constraints and filters:

```csharp
// Ensure conditions are met
public Result<User> GetActiveUser(int userId)
{
    return GetUser(userId)
        .Ensure(user => user.IsActive, userNotActiveKey, userId)
        .Ensure(user => !user.IsDeleted, userDeletedKey, userId);
}

// Age validation example
public Result<User> RegisterUser(CreateUserRequest request)
{
    return CreateUser(request)
        .Ensure(user => user.Age >= 18, underageKey, user.Age)
        .Ensure(user => user.Age <= 120, invalidAgeKey, user.Age);
}

// Business rule validation
public Result<Order> CreateOrder(CreateOrderRequest request)
{
    return ValidateOrderRequest(request)
        .Map(validRequest => new Order(validRequest))
        .Ensure(order => order.Total > 0, emptyOrderKey)
        .Ensure(order => order.Items.All(i => i.Quantity > 0), invalidQuantityKey)
        .Filter(order => order.Customer.CreditLimit >= order.Total, creditLimitKey, order.Total);
}

// Permission checking
public Result<Document> GetDocument(int documentId, int userId)
{
    return GetDocumentById(documentId)
        .Ensure(doc => doc.OwnerId == userId || doc.IsPublic, accessDeniedKey, documentId);
}
```

### Side Effects with Do Operations

Perform side effects without breaking the Result chain:

```csharp
// Logging successful operations
public async Task<Result<Order>> ProcessOrderAsync(CreateOrderRequest request)
{
    return await ValidateOrderRequest(request)
        .MapAsync(CreateOrderAsync)
        .DoAsync(async order => await _logger.LogAsync($"Order {order.Id} created"))
        .BindAsync(SendConfirmationEmailAsync)
        .DoAsync(async order => await _analytics.TrackOrderAsync(order));
}

// Caching results
public Result<User> GetUserWithCache(int userId)
{
    return GetUser(userId)
        .Do(user => _cache.Set($"user_{userId}", user, TimeSpan.FromMinutes(30)))
        .Do(user => _metrics.IncrementCounter("user_cache_miss"));
}

// Audit trail
public Result<User> UpdateUser(int userId, UpdateUserRequest request)
{
    return GetUser(userId)
        .Do(user => _auditService.LogUserAccess(userId, "update_attempt"))
        .Bind(user => ValidateUpdateRequest(user, request))
        .Map(user => ApplyUpdates(user, request))
        .Do(user => _auditService.LogUserUpdate(user))
        .Bind(SaveUser);
}

// Multi-step process with logging
public async Task<Result<Invoice>> GenerateInvoiceAsync(int orderId)
{
    return await GetOrder(orderId)
        .DoAsync(async order => await _logger.LogInfoAsync($"Generating invoice for order {order.Id}"))
        .MapAsync(CalculateInvoiceAsync)
        .DoAsync(async invoice => await _logger.LogInfoAsync($"Invoice calculated: {invoice.Total}"))
        .BindAsync(SaveInvoiceAsync)
        .DoAsync(async invoice => await _emailService.SendInvoiceAsync(invoice));
}
```

### Contextual Error Enhancement

Add context to errors for better debugging and user experience:

```csharp
// Add operation context to errors
public Result<User> RegisterUser(CreateUserRequest request)
{
    return ValidateEmail(request.Email)
        .WithContext("Email Validation")
        .Bind(_ => ValidatePassword(request.Password))
        .WithContext("Password Validation")
        .Bind(_ => CreateUser(request))
        .WithContext("User Creation");
}

// Service layer context
public class OrderService
{
    public Result<Order> CreateOrder(CreateOrderRequest request)
    {
        return ValidateOrderRequest(request)
            .WithContext($"Order validation for customer {request.CustomerId}")
            .Bind(CreateOrderFromRequest)
            .WithContext("Order creation")
            .Bind(AssignInventory)
            .WithContext("Inventory assignment");
    }
}

// API layer context
[HttpPost("users")]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    return await _userService.CreateUserAsync(request)
        .WithContext($"API: Creating user with email {request.Email}")
        .MatchAsync(
            onSuccess: user => Ok(user),
            onFailure: errors => BadRequest(new { 
                Message = "User creation failed",
                Errors = errors.Select(e => e.MapToErrorMessage()) 
            })
        );
}

// Repository layer context
public class UserRepository
{
    public Result<User> GetById(int id)
    {
        return Result.Try(
            () => _context.Users.Find(id),
            databaseErrorKey,
            id
        ).WithContext($"Database: Fetching user {id}");
    }
}
```

### Caching and Memoization

Cache expensive operations with automatic Result handling:

```csharp
// Cache expensive calculations
public Result<ComplexCalculationResult> GetComplexCalculation(int inputId)
{
    return (() => PerformComplexCalculation(inputId))
        .Memoize($"calculation_{inputId}", TimeSpan.FromHours(1));
}

// Cache API calls
public async Task<Result<WeatherData>> GetWeatherDataAsync(string cityCode)
{
    return await (async () => await _weatherApi.GetWeatherAsync(cityCode))
        .MemoizeAsync($"weather_{cityCode}", TimeSpan.FromMinutes(15));
}

// Cache user permissions
public Result<UserPermissions> GetUserPermissions(int userId)
{
    return (() => _permissionService.CalculatePermissions(userId))
        .Memoize($"permissions_{userId}", TimeSpan.FromMinutes(30));
}

// Service layer with caching
public class ProductService
{
    public Result<Product> GetProduct(int productId)
    {
        return (() => 
        {
            var product = _repository.GetById(productId);
            if (product == null)
                return Result.Error<Product>(productNotFoundKey, productId);
            return Result.Ok(product);
        }).Memoize($"product_{productId}", TimeSpan.FromMinutes(10));
    }
}
```

### Result Aggregation

Handle complex multi-step validation and processing:

```csharp
// Aggregate multiple validation results
public Result<User> ValidateAndCreateUser(CreateUserRequest request)
{
    var aggregator = new ResultAggregator<ValidationResult>();
    
    aggregator
        .Add(ValidateEmail(request.Email))
        .Add(ValidatePassword(request.Password))
        .Add(ValidateUsername(request.Username))
        .Add(ValidateAge(request.Age));
    
    return aggregator.ToResult()
        .Map(validationResults => CreateUser(request));
}

// Collect partial results
public Result<OrderSummary> ProcessOrderBatch(IEnumerable<CreateOrderRequest> requests)
{
    var aggregator = new ResultAggregator<Order>();
    
    foreach (var request in requests)
    {
        var orderResult = CreateOrder(request);
        aggregator.Add(orderResult);
    }
    
    var ordersResult = aggregator.ToResult();
    return ordersResult.Map(orders => new OrderSummary(orders));
}

// Handle mixed success/failure scenarios
public Result<BulkOperationResult> BulkUpdateUsers(IEnumerable<UpdateUserRequest> requests)
{
    var aggregator = new ResultAggregator<User>();
    
    foreach (var request in requests)
    {
        var updateResult = UpdateUser(request.UserId, request);
        aggregator.Add(updateResult);
    }
    
    // Even if some fail, we want to know about successes
    var result = aggregator.ToResult();
    return Result.Ok(new BulkOperationResult
    {
        SuccessfulUpdates = aggregator.SuccessfulResults,
        FailedUpdates = aggregator.Errors,
        TotalProcessed = requests.Count()
    });
}

// Single result from multiple attempts
public Result<User> FindUserByAnyIdentifier(string email, string username, string phoneNumber)
{
    var aggregator = new ResultAggregator<User>();
    
    aggregator
        .Add(FindUserByEmail(email))
        .Add(FindUserByUsername(username))
        .Add(FindUserByPhone(phoneNumber));
    
    return aggregator.ToSingleResult(); // Returns first successful result or all errors
}
```

### Merging Results

```csharp
// Merge multiple results
var result1 = ValidateUsername(username);
var result2 = ValidateEmail(email);
var result3 = ValidatePassword(password);

var combinedResult = result1.Merge(result2).Merge(result3);

// If any of the validation failed, the combined result will be unsuccessful
if (combinedResult.IsSuccessful)
{
    // All validations passed
}

// Working with multiple typed results
var userResults = new List<Result<User>>();
foreach (var userId in userIds)
{
    userResults.Add(GetUser(userId));
}

// Merge multiple typed results
var mergedUsers = userResults.MergeResults();
if (mergedUsers.IsSuccessful)
{
    // Process all users
    foreach (var user in mergedUsers.Value)
    {
        // Do something with each user
    }
}
```

### Method Chaining

```csharp
public Result<User> RegisterUser(string username, string email, string password)
{
    return ValidateInput(username, email, password)
        .OnSuccess(() => CreateUser(username, email, password))
        .OnSuccess(user => SendWelcomeEmail(user));
}
```

### Map and Bind Operations

```csharp
// Map transforms a successful Result<T> to a Result<U> using a mapper function
public Result<string> GetFormattedUsername(int userId)
{
    return GetUser(userId)
        .Map(user => $"{user.FirstName} {user.LastName}");
}

// MapAsync for async operations
public async Task<Result<UserViewModel>> GetUserViewModelAsync(int userId)
{
    return await _userRepository.GetUserAsync(userId)
        .MapAsync(async user => {
            var roles = await _roleRepository.GetRolesForUserAsync(user.Id);
            return new UserViewModel(user, roles);
        });
}

// Bind combines Results together in a chain
public Result<Order> CreateOrder(CreateOrderRequest request)
{
    return ValidateRequest(request)
        .Bind(validRequest => CreateOrderFromRequest(validRequest))
        .Bind(order => AssignInventory(order));
}

// BindAsync for async operations
public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request)
{
    return await ValidateRequestAsync(request)
        .BindAsync(async validRequest => await CreateOrderFromRequestAsync(validRequest))
        .BindAsync(async order => await AssignInventoryAsync(order));
}
```

### Validation Pipeline

```csharp
public class OrderService
{
    private readonly ValidationPipeline<Order> _orderValidation;

    public OrderService()
    {
        _orderValidation = new ValidationPipeline<Order>()
            .AddRule(ValidateCustomer)
            .AddRule(ValidateItems)
            .AddRule(ValidatePayment);
    }

    public async Task<Result<Order>> ProcessOrderAsync(CreateOrderRequest request)
    {
        return await Result.Ok(request)
            .Map(MapToOrder)
            .BindAsync(_orderValidation.Validate)
            .BindAsync(SaveOrderAsync)
            .BindAsync(SendConfirmationAsync);
    }
}
```

### Match Pattern

```csharp
public class UserController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        
        return result.Match(
            onSuccess: user => Ok(user),
            onFailure: errors => BadRequest(new { Errors = errors.Select(e => e.MapToErrorMessage()) })
        );
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request)
    {
        return await _userService.CreateUserAsync(request)
            .MatchAsync(
                onSuccess: async user => {
                    await _emailService.SendWelcomeEmailAsync(user.Email);
                    return Ok(user);
                },
                onFailure: async errors => {
                    await _auditService.LogValidationFailureAsync(request, errors);
                    return BadRequest(new { Errors = errors });
                });
    }
}

public async Task<string> ProcessOrderAsync(Order order)
{
    return await _orderRepository.SaveAsync(order)
        .MatchAsync(
            onSuccess: async savedOrder => {
                await _logService.LogSuccessAsync($"Order {savedOrder.Id} created");
                return $"Order {savedOrder.Id} processed successfully";
            },
            onFailure: async errors => {
                await _logService.LogErrorAsync("Order creation failed", errors);
                return "Order processing failed";
            });
}
```

## Advanced Patterns

### Repository Pattern with Result

```csharp
public class UserRepository
{
    public Result<User> GetById(int id)
    {
        if (id <= 0)
            return ValidationMessage.CreateError(invalidIdKey, id);
            
        return Result.Try(
            () => _context.Users.Find(id) ?? 
                  throw new InvalidOperationException("User not found"),
            userNotFoundKey,
            id
        );
    }
    
    public async Task<Result<User>> SaveAsync(User user)
    {
        return await Result.TryAsync(async () =>
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }, saveErrorKey, user.Id);
    }
}
```

### Service Layer Patterns

```csharp
public class OrderService
{
    public async Task<Result<Order>> ProcessOrderAsync(CreateOrderRequest request)
    {
        // Using all the advanced features together
        return await ValidateOrderRequest(request)
            .WithContext("Order Request Validation")
            .BindAsync(async validRequest => await CreateOrderAsync(validRequest))
            .WithContext("Order Creation")
            .DoAsync(async order => await _logger.LogInfoAsync($"Order {order.Id} created"))
            .BindAsync(async order => await ProcessPaymentAsync(order))
            .WithContext("Payment Processing")
            .Ensure(order => order.PaymentStatus == PaymentStatus.Completed, paymentFailedKey)
            .DoAsync(async order => await _inventory.ReserveItemsAsync(order.Items))
            .BindAsync(async order => await SendConfirmationAsync(order))
            .WithContext("Order Confirmation");
    }
}
```

### Error Recovery Patterns

```csharp
public class ResilientService
{
    public async Task<Result<Data>> GetDataWithFallbackAsync(int id)
    {
        // Try primary source first
        var primaryResult = await GetFromPrimarySourceAsync(id);
        if (primaryResult.IsSuccessful)
            return primaryResult;
            
        // Try secondary source on failure
        var secondaryResult = await GetFromSecondarySourceAsync(id);
        if (secondaryResult.IsSuccessful)
            return secondaryResult;
            
        // Aggregate all errors if both fail
        var aggregator = new ResultAggregator<Data>();
        aggregator.Add(primaryResult).Add(secondaryResult);
        return aggregator.ToSingleResult();
    }
}
```