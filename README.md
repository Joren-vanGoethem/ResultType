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

### match pattern

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