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

The system uses `TranslationKeyDefinition` to ensure type-safe validation messages, requiring users to provide the
correct number and types of parameters when creating validation messages.

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
        return Result.Create(ValidationMessage.CreateError(ErrorKey, input));
    }
    
    return Result.CreateSuccess();
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
public Result CreateUser(string username, string email) { 
    var validationMessages = new List  ();  
    // Validation logic...
    
    if (validationMessages.Any())
    {
        return Result.Create<User>(validationMessages);
    }
    
    var user = new User 
    {
        Username = username, 
        Email = email
    };
    return Result.Create(user);
}

``` 

## Validation Messages

### TranslationKeyDefinition

Used to define type-safe validation message keys with strongly-typed parameters.

**Example:**

```c# 
private static readonly TranslationKeyDefinition UsernameInvalidKey = TranslationKeyDefinition
        .Create("user.username.invalid") 
        .WithStringParameter("username")
        .WithIntParameter("minLength");

// Usage 
var message = ValidationMessage.CreateError(UsernameInvalidKey, "jo", 3);
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

### Memoization Extensions

- `Memoize()`: Cache result computations
- Result-specific memoization for expensive operations

## Practical Usage Examples

### Service Layer Validation

```c# 
public class UserService {
    private static readonly TranslationKeyDefinition EmailInvalidKey = 
        TranslationKeyDefinition
            .Create("user.email.invalid")
            .WithStringParameter("email");
    
    public Result<User> ValidateUser(User user)
    {
        var validationMessages = new List<ValidationMessage>();
    
        if (!IsValidEmail(user.Email))
        {
            validationMessages.Add(ValidationMessage.CreateError(EmailInvalidKey, user.Email));
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

### 1. Use Strongly-Typed Translation Keys

Always define translation keys with proper parameter types to ensure type safety:

```c# 
// Good 
private static readonly TranslationKeyDefinition AgeInvalidKey = TranslationKeyDefinition
    .Create("user.age.invalid") 
    .WithIntParameter("age") 
    .WithIntParameter("minAge");

// Usage 
ValidationMessage.CreateError(AgeInvalidKey, user.Age, 18);
``` 

### 2. Collect All Validation Messages

Don't fail fast - collect all validation issues to provide comprehensive feedback:

```c# 
public Result ValidateUser(User user) { 
    var messages = new List();  
    
    // Validate all fields
    if (string.IsNullOrEmpty(user.Username))
        messages.Add(ValidationMessage.CreateError(UsernameRequiredKey));
    
    if (string.IsNullOrEmpty(user.Email))
        messages.Add(ValidationMessage.CreateError(EmailRequiredKey));
    
    if (user.Age < 18)
        messages.Add(ValidationMessage.CreateError(AgeInvalidKey, user.Age, 18));
    
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