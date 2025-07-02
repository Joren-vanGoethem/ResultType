# Validation-System

## Overview

The Validation Pipeline system provides a structured approach to building and executing complex validation scenarios. It
allows you to compose multiple validation rules into a single pipeline that can be executed against objects, collecting
all validation messages in a single operation.

## Core Components

### ValidationPipeline

The `ValidationPipeline<T>` class is the main component that orchestrates the validation process. It allows you to:

- Add multiple validation rules
- Execute all rules against an object
- Collect and merge validation results
- Return a comprehensive validation result

## Basic Usage

### Creating a Validation Pipeline

```c#
var pipeline = new ValidationPipeline<User>()
    .AddRule(user => ValidateUsername(user))
    .AddRule(user => ValidateEmail(user))
    .AddRule(user => ValidateAge(user));
```

### Executing Validation

```c#
var result = pipeline.Validate(user);

if (result.IsSuccessful)
{
// All validations passed
    var validatedUser = result.Value;
}
else
{
// One or more validations failed
    var errors = result.ValidationMessages;
}
```

## Validation Rules

### Creating Individual Validation Rules

Validation rules are functions that take an object of type `T` and return a `Result`:

```c#
private static Result ValidateUsername(User user)
{
    if (string.IsNullOrWhiteSpace(user.Username))
    {
        return Result.Create(ValidationMessage.CreateError(UsernameRequiredKey));
    }
    
        if (user.Username.Length < 3)
        {
            return Result.Create(ValidationMessage.CreateError(UsernameInvalidKey, user.Username, 3));
        }
        
        return Result.CreateSuccess();
    }
    
    private static Result ValidateEmail(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return Result.Create(ValidationMessage.CreateError(EmailRequiredKey));
        }
    
        if (!IsValidEmail(user.Email))
        {
            return Result.Create(ValidationMessage.CreateError(EmailInvalidKey, user.Email));
        }
        
        return Result.CreateSuccess();
}
```

### Complex Validation Rules

Rules can contain complex logic and multiple validation checks:

```c#
private static Result ValidateUserProfile(User user)
{
    var messages = new List<ValidationMessage>();

    // Multiple related validations
    if (user.Age < 13)
    {
        messages.Add(ValidationMessage.CreateError(AgeMinimumKey, user.Age, 13));
    }
    
    if (user.Age < 18 && string.IsNullOrEmpty(user.ParentEmail))
    {
        messages.Add(ValidationMessage.CreateError(ParentEmailRequiredKey));
    }
    
    return messages.Any() 
        ? Result.Create(messages) 
        : Result.CreateSuccess();
}
```

## Translation Key Definitions

### Type-Safe Parameter Validation

The validation system uses `TranslationKeyDefinition` to ensure type-safe validation messages:

```c#
// Define keys with strongly-typed parameters
private static readonly TranslationKeyDefinition UsernameInvalidKey = TranslationKeyDefinition
    .Create("user.username.invalid")
    .WithStringParameter("username")
    .WithIntParameter("minLength");

private static readonly TranslationKeyDefinition EmailInvalidKey = TranslationKeyDefinition
    .Create("user.email.invalid")
    .WithStringParameter("email");

private static readonly TranslationKeyDefinition AgeInvalidKey = TranslationKeyDefinition
    .Create("user.age.invalid")
    .WithIntParameter("age")
    .WithIntParameter("minAge");
```

### Parameter Type Support

The system supports various parameter types:

```c#
var keyDefinition = TranslationKeyDefinition
    .Create("validation.complex")
    .WithStringParameter("name")
    .WithIntParameter("count")
    .WithBoolParameter("isActive")
    .WithDateTimeParameter("createdAt")
    // careful here, it will validate that it is a correct email, 
    // so use string if you expect the email to be wrongly formed
    .WithEmailParameter("email")
    .WithUriParameter("website");
```

## Validation Message Types

### Error Messages

Critical validation failures that prevent processing:

```c#
ValidationMessage.CreateError(UsernameInvalidKey, user.Username, 3);
```

### Warning Messages

Non-critical issues that don't prevent processing:

```c#
ValidationMessage.CreateWarning(PasswordWeakKey, user.Username);
```

### Information Messages

Informational messages for user feedback:

```c#
ValidationMessage.CreateInfo(AccountCreatedKey, user.Username);
```

## Advanced Pipeline Patterns

### Conditional Validation Rules

```c#
var pipeline = new ValidationPipeline<User>()
    .AddRule(user => ValidateBasicInfo(user))
    .AddRule(user => user.IsAdmin ? ValidateAdminRights(user) : Result.CreateSuccess())
    .AddRule(user => user.Age < 18 ? ValidateMinorRequirements(user) : Result.CreateSuccess());
```

### Reusable Validation Components

```c#
public static class UserValidationRules
{
    public static ValidationPipeline<User> CreateBasicValidation()
    {
        return new ValidationPipeline<User>()
            .AddRule(ValidateUsername)
            .AddRule(ValidateEmail)
            .AddRule(ValidateAge);
    }

    public static ValidationPipeline<User> CreateExtendedValidation()
    {
        return CreateBasicValidation()
            .AddRule(ValidatePhoneNumber)
            .AddRule(ValidateAddress)
            .AddRule(ValidatePreferences);
    }
}
```

### Async Validation Rules

While the current pipeline is synchronous, you can create async versions:

```c#
public class AsyncValidationPipeline<T>
{
    private readonly List<Func<T, Task<Result>>> _validators = new();

    public AsyncValidationPipeline<T> AddRule(Func<T, Task<Result>> validator)
    {
        _validators.Add(validator);
        return this;
    }

    public async Task<Result<T>> ValidateAsync(T value)
    {
        var tasks = _validators.Select(v => v(value));
        var results = await Task.WhenAll(tasks);
        var mergedResult = results.MergeResults();

        return mergedResult.IsSuccessful
            ? Result.Create(value)
            : Result.Create<T>(mergedResult.ValidationMessages);
    }
}
```

## Best Practices

### 1. Organize Validation Logic

Group related validation rules together and use descriptive names:

```c#
public static class UserValidators
{
    public static Result ValidateIdentity(User user) { /* ... */ }
    public static Result ValidateContact(User user) { /* ... */ }
    public static Result ValidatePermissions(User user) { /* ... */ }
}
```

### 2. Define Translation Keys Consistently

Use a consistent naming convention for translation keys:

```c#
// Domain.Entity.Property.ValidationRule
private static readonly TranslationKeyDefinition UsernameRequiredKey =
    TranslationKeyDefinition.Create("user.username.required");

private static readonly TranslationKeyDefinition EmailFormatKey =
    TranslationKeyDefinition.Create("user.email.format");
```

### 3. Fail Fast vs. Collect All

The pipeline collects all validation messages by default. For expensive validations, consider short-circuiting:

```c#
private static Result ExpensiveValidation(User user)
{
    // Only run expensive validation if basic validation passes
    var basicResult = ValidateBasicInfo(user);
    if (basicResult.IsFailure)
    return basicResult;
    
    // Proceed with expensive validation
    return PerformExpensiveChecks(user);
}
```

## Integration Examples

### ASP.NET Core Integration

```c#
[HttpPost]
public IActionResult CreateUser([FromBody] CreateUserRequest request)
{
    var user = mapper.Map<User>(request);
    var validationResult = userValidationPipeline.Validate(user);
    
    if (validationResult.IsFailure)
    {
        return BadRequest(validationResult.ValidationMessages);
    }
    
    var createdUser = userService.Create(validationResult.Value);
    return Ok(createdUser);
}
```


