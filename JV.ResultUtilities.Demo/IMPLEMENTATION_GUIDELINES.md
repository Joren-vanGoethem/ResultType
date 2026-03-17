# Implementation Guidelines: JV.ResultUtilities in API Projects

## 1. Project Setup

### NuGet and Localization

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Localization — required for translating validation messages
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supported = new[] { "en", "nl" }; // add your supported cultures
    options.SetDefaultCulture("en");
    options.AddSupportedCultures(supported);
    options.AddSupportedUICultures(supported);
});

// ProblemDetails + ResultExceptionHandler — catches ThrowIfFailure() exceptions
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ResultExceptionHandler>();

// Override model binding errors to use the same 422 format
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Validation Failed",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.21"
        };
        return new UnprocessableEntityObjectResult(problemDetails);
    };
});

var app = builder.Build();

app.UseExceptionHandler();        // must come before routing
app.UseRequestLocalization();     // reads Accept-Language header automatically
app.MapControllers();
```

`UseRequestLocalization()` reads the `Accept-Language` header and sets `CultureInfo.CurrentUICulture` for the request. `IStringLocalizer` then automatically resolves the correct `.resx` file. No manual header parsing needed.

### Consistent Error Response Shape

Every validation failure — whether from Result validation, model binding, or a thrown `ResultException` — produces the same `ValidationProblemDetails` with HTTP 422:

```json
{
  "status": 422,
  "title": "Validation Failed",
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "errors": {
    "product.name.length": ["Product name 'AB' must be between 3 and 100 characters."]
  }
}
```

---

## 2. Implicit Operators — Let the Type System Work

`Result<T>` has three implicit conversions that eliminate most explicit `Result.Ok()` / `Result.Error()` calls:

| Expression returned | Implicit conversion | Equivalent to |
|---|---|---|
| `return product;` | `TValue -> Result<TValue>` | `return Result.Ok(product);` |
| `return validationMessage;` | `ValidationMessage -> Result<TValue>` | `return Result.Error(validationMessage);` |
| `return messageArray;` | `ValidationMessage[] -> Result<TValue>` | `return Result.Error(messages);` |
| `return Result.Error(key, params);` | `Result -> Result<TValue>` | works for failures only |

### When you still need explicit calls

- **`Result.Ok()`** — non-generic `Result` (no value to trigger an implicit conversion)
- **`Result.Error(key, params)`** — in validation rules that return `Result` (non-generic)
- **`Result.Try()` / `Result.TryAsync()`** — wrapping code that may throw

### Examples

```csharp
// Service returning Result<ProductResponse>
public async Task<Result<ProductResponse>> GetByIdAsync(Guid id)
{
    var result = await repository.GetByIdAsync(id);
    if (result.IsFailure)
        return result.Cast<ProductResponse>();  // forward failure to different type

    // Implicit: bare ProductResponse -> Result<ProductResponse>
    return ToResponse(result.Value);
}

// Returning a validation message directly
public async Task<Result<OrderResponse>> GetByIdAsync(Guid id)
{
    var orderResult = await repository.GetByIdAsync(id);
    if (orderResult.IsFailure)
        // Implicit: ValidationMessage -> Result<OrderResponse>
        return ValidationMessage.Create(OrderValidationKeys.NotFound, id);

    return ToResponse(orderResult.Value);
}
```

---

## 3. Static Create Methods on Entities

Use a static `Create` method that returns `Result<T>` to enforce domain invariants at construction time. The entity validates itself and returns either a valid instance or validation errors — invalid entities can never exist.

```csharp
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Sku { get; private set; }
    public decimal Price { get; private set; }
    public ProductCategory Category { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Product() { } // EF Core needs this

    public static Result<Product> Create(
        string name, string sku, decimal price,
        ProductCategory category)
    {
        // Use a validation pipeline for complex rules
        var pipeline = new ValidationPipeline<(string Name, string Sku, decimal Price, ProductCategory Category)>()
            .AddRule(r => string.IsNullOrWhiteSpace(r.Name)
                ? Result.Error(ProductValidationKeys.NameRequired)
                : Result.Ok())
            .AddRule(r => r.Name.Length is < 3 or > 100
                ? Result.Error(ProductValidationKeys.NameLength, new object[] { r.Name, 3, 100 })
                : Result.Ok())
            .AddRule(r => r.Price <= 0
                ? Result.Error(ProductValidationKeys.PriceInvalid, 0m)
                : Result.Ok())
            .AddRule(r => string.IsNullOrWhiteSpace(r.Sku)
                ? Result.Error(ProductValidationKeys.SkuRequired)
                : Result.Ok());

        var validationResult = pipeline.Validate((name, sku, price, category));
        if (validationResult.IsFailure)
            return validationResult.Cast<Product>();

        // Implicit: Product -> Result<Product>
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Sku = sku,
            Price = price,
            Category = category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

The service then chains Create into the rest of the flow:

```csharp
public async Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request)
{
    return await Product.Create(request.Name, request.Sku, request.Price, request.Category)
        .BindAsync(async product => await repository.Add(product))
        .MapSync(ToResponse);
}
```

If you prefer to keep the validation pipeline separate from the entity (e.g. when validation requires injected services or database lookups), validate the request first, then call `Create`:

```csharp
public async Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request)
{
    var validationResult = ProductValidator.CreatePipeline().Validate(request);
    if (validationResult.IsFailure)
        return validationResult.Cast<ProductResponse>();

    return Product.Create(request.Name, request.Sku, request.Price, request.Category)
        .Bind(product => repository.Add(product))
        .Map(ToResponse);
}
```

---

## 4. Validation Pipeline

### Defining Validation Keys

Each domain gets a static class with `ValidationKeyDefinition` constants. The key doubles as the `.resx` lookup key.

```csharp
public static class ProductValidationKeys
{
    public static readonly ValidationKeyDefinition NameRequired =
        ValidationKeyDefinition.Create("product.name.required");

    public static readonly ValidationKeyDefinition NameLength =
        ValidationKeyDefinition.Create("product.name.length")
            .WithStringParameter("name")
            .WithIntParameter("minLength")
            .WithIntParameter("maxLength");

    public static readonly ValidationKeyDefinition PriceInvalid =
        ValidationKeyDefinition.Create("product.price.invalid")
            .WithDecimalParameter("minPrice");

    // Use .WithFieldName() to control the error key in ProblemDetails
    public static readonly ValidationKeyDefinition SkuRequired =
        ValidationKeyDefinition.Create("product.sku.required")
            .WithFieldName("sku");
}
```

The parameter types (`WithStringParameter`, `WithIntParameter`, `WithDecimalParameter`, `WithGuidParameter`, `WithEmailParameter`, etc.) enforce type safety — `ValidationMessage.Create` will throw `ArgumentException` at runtime if the wrong types are passed.

### Building a Pipeline

```csharp
public static class ProductValidator
{
    public static ValidationPipeline<CreateProductRequest> CreatePipeline()
    {
        return new ValidationPipeline<CreateProductRequest>()
            .AddRule(ValidateName)
            .AddRule(ValidatePrice)
            .AddRule(ValidateSku);
    }

    // Each rule returns Result (non-generic)
    private static Result ValidateName(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Error(ProductValidationKeys.NameRequired);

        if (request.Name.Length < 3 || request.Name.Length > 100)
            return Result.Error(ProductValidationKeys.NameLength,
                new object[] { request.Name, 3, 100 });

        return Result.Ok();
    }

    private static Result ValidatePrice(CreateProductRequest request)
    {
        return request.Price > 0
            ? Result.Ok()
            : Result.Error(ProductValidationKeys.PriceInvalid, 0m);
    }

    private static Result ValidateSku(CreateProductRequest request)
    {
        return string.IsNullOrWhiteSpace(request.Sku)
            ? Result.Error(ProductValidationKeys.SkuRequired)
            : Result.Ok();
    }
}
```

### Sync vs Async Rules

- **All sync rules** — call `pipeline.Validate(request)` — returns `Result<T>`
- **Any async rule** — call `pipeline.ValidateAsync(request)` — calling `Validate()` throws `InvalidOperationException`
- **ShortCircuit** — set `pipeline.ShortCircuit = true` to stop on first failure

```csharp
// Async rule example: checks database
var pipeline = new ValidationPipeline<CreateOrderRequest>()
    .AddRule(ValidateCustomerName)           // sync
    .AddRule(ValidateStockAvailability);     // async — must use ValidateAsync

private static async Task<Result> ValidateStockAvailability(CreateOrderRequest request)
{
    var product = await db.Products.FindAsync(request.ProductId);
    if (product is null)
        return Result.Error(OrderValidationKeys.ProductNotFound, request.ProductId);
    if (product.StockQuantity < request.Quantity)
        return Result.Error(OrderValidationKeys.InsufficientStock,
            new object[] { product.StockQuantity, product.Name, request.Quantity });
    return Result.Ok();
}

// Usage — MUST be ValidateAsync because of async rule
var result = await pipeline.ValidateAsync(request);
```

---

## 5. Translation

### Resource Files

Create `.resx` files under `Resources/` with the `TranslationKey` as the name:

**`Resources/ValidationMessages.resx`** (default/English):
```xml
<data name="product.name.length" xml:space="preserve">
  <value>Product name '{0}' must be between {1} and {2} characters.</value>
</data>
```

**`Resources/ValidationMessages.nl.resx`** (Dutch):
```xml
<data name="product.name.length" xml:space="preserve">
  <value>Productnaam '{0}' moet tussen {1} en {2} tekens zijn.</value>
</data>
```

Parameters use positional `{0}`, `{1}`, `{2}` placeholders matching the order defined in `ValidationKeyDefinition`. `RawParameters` (original typed values) are passed to the localizer, so numbers and dates format according to the request culture.

### Marker Class

A marker class is required in the **root namespace** (not `Resources`):

```csharp
namespace YourProject;

// Must be in root namespace so ResourceManagerStringLocalizerFactory
// resolves: {RootNamespace}.Resources.ValidationMessages
public class ValidationMessages;
```

### How Accept-Language Flows Through

```
Client sends: Accept-Language: nl
         |
UseRequestLocalization() sets CultureInfo.CurrentUICulture = "nl"
         |
Controller injects IStringLocalizer<ValidationMessages>
         |
BuildValidationProblemDetails calls localizer["product.name.length", rawParams]
         |
ResourceManager loads ValidationMessages.nl.resx
         |
String.Format applies culture-aware formatting (decimal separators, dates, etc.)
         |
Client receives: "Productnaam 'AB' moet tussen 3 en 100 tekens zijn."
```

No manual header parsing is needed. `UseRequestLocalization()` handles it.

---

## 6. Controllers — Result to HTTP

Inject `IStringLocalizer<ValidationMessages>` and use the extension methods:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    ProductService productService,
    IStringLocalizer<ValidationMessages> localizer) : ControllerBase
{
    // Standard: 200 OK or 422
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await productService.GetAllAsync()
            .ToActionResultAsync(localizer);
    }

    // Custom success: 201 Created
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        return await productService.CreateAsync(request)
            .ToActionResultAsync(localizer,
                product => CreatedAtAction(nameof(GetById), new { id = product.Id }, product));
    }

    // Non-generic Result: 204 No Content or 422
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await productService.DeactivateAsync(id)
            .ToActionResultAsync(localizer);
    }

    // Custom logic: 404 for not-found, 422 for validation errors
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await productService.GetByIdAsync(id);
        return result.Match(
            onSuccess: product => (IActionResult)Ok(product),
            onFailure: messages =>
            {
                if (messages.Any(m => m.KeyDefinition?.Key == "product.not_found"))
                    return NotFound();
                return result.ToActionResult(localizer);
            });
    }
}
```

### Extension Methods Summary

| Method | Success | Failure |
|---|---|---|
| `result.ToActionResult(localizer)` | `204 No Content` | `422 + ProblemDetails` |
| `result.ToActionResult<T>(localizer)` | `200 OK` with value | `422 + ProblemDetails` |
| `result.ToActionResult<T>(localizer, onSuccess)` | Custom (e.g. 201) | `422 + ProblemDetails` |
| Async variants: `.ToActionResultAsync(...)` | Same | Same |

---

## 7. ResultExceptionHandler — The Safety Net

For code paths where returning `Result` is impractical (deep call chains, event handlers), throw instead:

```csharp
// In service code — throws ResultException if result is a failure
var merged = result1.Merge(result2);
merged.ThrowIfFailure();  // throws ResultException with ValidationMessages

// The middleware catches it and returns 422 ProblemDetails
```

Register in `Program.cs`:

```csharp
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ResultExceptionHandler>();
// ...
app.UseExceptionHandler();
```

The `ResultExceptionHandler` catches `ResultException`, calls the same `BuildValidationProblemDetails` method, and writes a 422 response. The error shape is identical to the explicit `ToActionResult` path.

---

## 8. Repository Layer — Result.Try / Result.TryAsync

Wrap database operations that may throw:

```csharp
public Result<Product> Add(Product product)
{
    return Result.Try(() =>
    {
        context.Products.Add(product);
        context.SaveChanges();
        return product;
    }, ProductValidationKeys.SaveFailed);
}

public async Task<Result<Order>> AddAsync(Order order)
{
    return await Result.TryAsync(async () =>
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order;
    }, OrderValidationKeys.SaveFailed);
}
```

The exception message is captured as the parameter value for the `SaveFailed` key.

---

## Quick Reference: What to Return Where

| Layer | Return type | Pattern |
|---|---|---|
| Validation rule | `Result` | `Result.Ok()` or `Result.Error(key, params)` |
| Entity `Create` | `Result<T>` | Implicit: return new entity, or `pipeline.Cast<T>()` |
| Repository | `Result<T>` | `Result.Try` / `Result.TryAsync` |
| Service | `Result<T>` or `Result` | Implicit returns, `Map`, `Bind`, `Cast<T>` |
| Controller | `IActionResult` | `.ToActionResult(localizer)` or `Match` |
