# Result-Types

## Overview

The Result types in this project provide a robust, type-safe error handling system that replaces traditional
exception-based error handling with a more functional approach. The system is designed around the concept that
operations can either succeed or fail, with failures being represented as validation messages rather than exceptions.

## Core Concepts

### Binary Result State

Results are binary - they are either:

- **Successful**: Contain no validation messages
- **Unsuccessful**: Contain one or more validation messages

### Type Safety

The system uses `ValidationKeyDefinition` to ensure type-safe validation messages, requiring users to provide the
correct number and types of parameters when creating validation messages.

### Parameter Passing

`ValidationKeyDefinition` supports multiple ways to provide parameters:
- **Positional**: Using `object[]` or direct values (if count matches).
- **Named (Anonymous Objects)**: `new { name = "John", age = 30 }`.
- **Named (Dictionary)**: `new Dictionary<string, object> { ["name"] = "John" }`.
- **Single Value**: Passing a single value directly if the definition has only one required parameter.
- **Default Values**: Parameters can have default values, making them optional when using named parameters.

See [Validation System](Validation-System.md#type-safe-parameter-validation) for more details.

## Result Type

### `Result` (Non-Generic)

A basic result type that indicates success or failure without carrying a value.

**Properties:**

- `IsSuccessful`: Boolean indicating if the operation succeeded
- `IsFailure`: Boolean indicating if the operation failed
- `ValidationMessages`: Collection of validation messages (empty for successful results)

**Usage:**

```c# 
// For operations that don't return a value 
public Result ValidateInput(string input) { 
    if (string.IsNullOrEmpty(input)) 
    {
        return Result.Error(ErrorKey, input);
    }
    
    return Result.Ok();
}
``` 

### `Result<T>` (Generic)

A generic result type that carries a value when successful.

**Properties:**

- `IsSuccessful`: Boolean indicating if the operation succeeded
- `IsFailure`: Boolean indicating if the operation failed
- `ValidationMessages`: Collection of validation messages
- `Value`: The actual value (only available when successful)

**Usage:**

```c# 
// For operations that return a value 
public Result<User> CreateUser(string username, string email) { 
    var validationMessages = new List<ValidationMessage>();  
    // Validation logic...
    
    if (validationMessages.Any())
    {
        return Result.Error(validationMessages);
    }
    
    var user = new User 
    {
        Username = username, 
        Email = email
    };
    return Result.Ok(user);
}

``` 

## Validation Messages

### ValidationKeyDefinition

Used to define type-safe validation message keys with strongly-typed parameters.

**Example:**

```c# 
private static readonly ValidationKeyDefinition UsernameInvalidKey = ValidationKeyDefinition
        .Create("user.username.invalid") 
        .WithStringParameter("username")
        .WithIntParameter("minLength");

// Usage 
var message = ValidationMessage.Create(UsernameInvalidKey, "jo", 3);
``` 

### ValidationMessage Types

- **Error**: Critical validation failures
- **Warning**: Non-critical issues
- **Info**: Informational messages

## Result Creation Patterns

### Success Results

```c# 
// Non-generic success 
var result = Result.CreateSuccess();

// Generic success with value 
var result = Result.Create(user);

// Success with informational messages 
var result = Result.Create(user, infoMessages);
``` 

### Failure Results

```c# 
// Single validation message 
var result = Result.Create(ValidationMessage.CreateError(key, parameters));

// Multiple validation messages 
var result = Result.Create(validationMessages);
``` 

## Extension Methods

The system provides several extension methods for working with results:

### Result Extensions

- `Map<TResult>()`: Transform successful results
- `MapAsync<TResult>()`: Asynchronous transformation
- `Bind<TResult>()`: Chain operations that return results
- `Match<TResult>()`: Pattern matching for success/failure

### Collection Extensions

- `MergeResults()`: Combine multiple results
- `AllSuccessful()`: Check if all results in a collection are successful
- `GetSuccessfulValues()`: Extract values from successful results

### Exception Handling

- `Try()`: Wrap synchronous operations and capture exceptions
- `TryAsync()`: Wrap asynchronous operations and capture exceptions

### Memoization Extensions

- `Memoize()`: Cache result computations
- Result-specific memoization for expensive operations

### Exception Handling with `Try` and `TryAsync`

The `Result.Try` and `Result.TryAsync` methods allow you to wrap operations that might throw exceptions, automatically converting those exceptions into failure results.

**`Result.Try<T>` (Synchronous)**

Executes a synchronous operation and returns its result as an `Ok(value)`. If an exception occurs, it returns an `Error` with the provided `ValidationKeyDefinition` and the exception message.

```c#
var key = ValidationKeyDefinition.Create("operation.failed").WithStringParameter("error");

var result = Result.Try(
    () => File.ReadAllText("config.json"),
    key
);
```

**`Result.TryAsync<T>` (Asynchronous)**

The asynchronous version for `Task`-based operations.

```c#
var result = await Result.TryAsync(
    async () => await httpClient.GetStringAsync("https://api.example.com/data"),
    key
);
```

> **Note**: When an exception is caught, the exception message is automatically appended as the last parameter to the provided `ValidationKeyDefinition`. Ensure your key definition includes a parameter for this message if you want it included in the formatted error.

## Practical Usage Examples

### Service Layer Validation

```c# 
public class UserService {
    private static readonly ValidationKeyDefinition EmailInvalidKey = 
        ValidationKeyDefinition
            .Create("user.email.invalid")
            .WithStringParameter("email");
    
    public Result<User> ValidateUser(User user)
    {
        var validationMessages = new List<ValidationMessage>();
    
        if (!IsValidEmail(user.Email))
        {
            validationMessages.Add(ValidationMessage.Create(EmailInvalidKey, user.Email));
        }
    
        return validationMessages.Any() 
            ? Result.Create<User>(validationMessages)
            : Result.Create(user);
    }
}
``` 

### Method Chaining

```c# 
var result = ValidateUser(user) 
    .Map(u => EnrichUser(u)) 
    .Bind(u => SaveUser(u)) 
    .Map(u => CreateUserDto(u));
``` 

### Handling Multiple Operations

```c# 
var results = new[] { result1, result2, result3 }
    .MergeResults(); 

if (results.IsSuccessful) { 
    // All operations succeeded 
    var allValues = results.Value; 
}
``` 

## Best Practices

### 1. Use Strongly-Typed Validation Keys

Always define validation keys with proper parameter types to ensure type safety:

```c# 
// Good 
private static readonly ValidationKeyDefinition AgeInvalidKey = ValidationKeyDefinition
    .Create("user.age.invalid") 
    .WithIntParameter("age") 
    .WithIntParameter("minAge");

// Usage 
ValidationMessage.Create(AgeInvalidKey, user.Age, 18);
``` 

### 2. Collect All Validation Messages

Don't fail fast - collect all validation issues to provide comprehensive feedback:

```c# 
public Result ValidateUser(User user) { 
    var messages = new List<ValidationMessage>();  
    
    // Validate all fields
    if (string.IsNullOrEmpty(user.Username))
        messages.Add(ValidationMessage.Create(UsernameRequiredKey));
    
    if (string.IsNullOrEmpty(user.Email))
        messages.Add(ValidationMessage.Create(EmailRequiredKey));
    
    if (user.Age < 18)
        messages.Add(ValidationMessage.Create(AgeInvalidKey, user.Age, 18));
    
    return messages.Any()
        ? Result.Create<User>(messages)
        : Result.Create(user);
}
``` 

### 3. Use Extension Methods for Composition

Leverage the provided extension methods for clean, functional-style composition:

```c# 
return ValidateUser(userData) 
    .Map(user => user with { Id = Guid.NewGuid() }) 
    .Bind(user => repository.SaveAsync(user)) 
    .Map(user => mapper.ToDto(user));
``` 

### 4. Handle Both Success and Failure Cases

Always handle both successful and failed results appropriately:

```c# 
var result = userService.CreateUser(request); 
return result.Match( 
    onSuccess: user => Ok(user), 
    onFailure: messages => BadRequest(messages) 
);
```

This Result type system provides a robust foundation for error handling that promotes clean, maintainable code while
ensuring comprehensive error reporting and type safety.