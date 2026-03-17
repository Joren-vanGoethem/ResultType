# Validation-System

## Overview

The Validation Pipeline system provides a structured approach to building and executing complex validation scenarios. It
allows you to compose multiple validation rules into a single pipeline that can be executed against objects, collecting
all validation messages in a single operation.

## Core Components

### ValidationPipeline

The `ValidationPipeline<T>` class is the main component that orchestrates the validation process. It allows you to:

- Add multiple sync and async validation rules
- Execute all rules against an object (or stop on first failure with `ShortCircuit`)
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

## Validation Key Definitions

### Type-Safe Parameter Validation

The validation system uses `ValidationKeyDefinition` to ensure type-safe validation messages. It supports several ways to provide parameters: positional, named (via anonymous objects or dictionaries), and default values.

```c#
// Define keys with strongly-typed parameters and optional default values
private static readonly ValidationKeyDefinition UsernameInvalidKey = ValidationKeyDefinition
    .Create("user.username.invalid")
    .WithStringParameter("username")
    .WithIntParameter("minLength", 3); // Default value of 3

private static readonly ValidationKeyDefinition EmailInvalidKey = ValidationKeyDefinition
    .Create("user.email.invalid")
    .WithStringParameter("email");

private static readonly ValidationKeyDefinition AgeInvalidKey = ValidationKeyDefinition
    .Create("user.age.invalid")
    .WithIntParameter("age")
    .WithIntParameter("minAge", 18);

// Optionally associate a field name for mapping errors to form fields
private static readonly ValidationKeyDefinition NameRequiredKey = ValidationKeyDefinition
    .Create("user.name.required")
    .WithFieldName("name");
```

### Parameter Passing Options

When creating a `ValidationMessage` or a `Result.Error`, you can pass parameters in multiple ways:

#### 1. Positional Parameters (Array)
Parameters are matched by their order in the definition.

```c#
// Matches "username" and "minLength"
Result.Error(UsernameInvalidKey, new object[] { "jsmith", 5 });
```

#### 2. Named Parameters (Anonymous Object)
Parameters are matched by property names. Order does not matter.

```c#
// Matches by name
Result.Error(UsernameInvalidKey, new { minLength = 5, username = "jsmith" });
```

#### 3. Named Parameters (Dictionary)
Useful when parameters are dynamically generated.

```c#
var params = new Dictionary<string, object> {
    { "username", "jsmith" },
    { "minLength", 5 }
};
Result.Error(UsernameInvalidKey, params);
```

#### 4. Default Values
If a parameter has a default value, it can be omitted when using named parameters.

```c#
// minLength will use its default value (3)
Result.Error(UsernameInvalidKey, new { username = "jsmith" });
```

#### 5. Single Value Passing
If a definition has only one parameter (or only one parameter without a default value), you can pass the value directly without an array or object.

```c#
// Automatically maps to "email" parameter
Result.Error(EmailInvalidKey, "invalid-email");

// Automatically maps to "username" because "minLength" has a default
Result.Error(UsernameInvalidKey, "jsmith");
```

### Parameter Type Support

The system supports various parameter types:

```c#
var keyDefinition = ValidationKeyDefinition
    .Create("validation.complex")
    .WithStringParameter("name")
    .WithIntParameter("count")
    .WithBooleanParameter("isActive")
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
ValidationMessage.Create(UsernameInvalidKey, user.Username, 3);
```

### Lenient Creation

When you need to create a message without parameter type validation (e.g., for dynamic parameters or bridging with external systems), use `CreateLenient`. Parameters are converted to strings via `ToString()`:

```c#
var message = ValidationMessage.CreateLenient(UsernameInvalidKey, "value1", 42, someObject);
```

### RawParameters

`ValidationMessage` exposes a `RawParameters` property that preserves the original typed parameter values before string formatting. This is useful when you need to programmatically inspect parameter values:

```c#
var message = ValidationMessage.Create(AgeInvalidKey, 15, 18);
// message.Parameters     -> ["15", "18"] (formatted strings)
// message.RawParameters  -> [15, 18]     (original typed values)
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

`ValidationPipeline<T>` supports both sync and async rules natively. Use `AddRule` with a `Func<T, Task<Result>>` for async rules, and `ValidateAsync()` to execute them:

```c#
var pipeline = new ValidationPipeline<User>()
    .AddRule(user => ValidateUsername(user))          // sync rule
    .AddRule(async user =>                            // async rule
    {
        var exists = await userRepository.ExistsAsync(user.Email);
        return exists
            ? Result.Error(EmailTakenKey, user.Email)
            : Result.Ok();
    });

var result = await pipeline.ValidateAsync(user);
```

> **Note**: Calling `Validate()` (synchronous) throws `InvalidOperationException` if any async rules have been added. Use `ValidateAsync()` when async rules are present.

### Short-Circuit Mode

By default, the pipeline collects all validation errors. Set `ShortCircuit = true` to stop on the first failure:

```c#
var pipeline = new ValidationPipeline<User> { ShortCircuit = true }
    .AddRule(user => ValidateBasicInfo(user))
    .AddRule(user => ValidateExpensiveCheck(user));  // skipped if first rule fails

// Works with both Validate() and ValidateAsync()
var result = pipeline.Validate(user);
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

### 2. Define Validation Keys Consistently

Use a consistent naming convention for validation keys:

```c#
// Domain.Entity.Property.ValidationRule
private static readonly ValidationKeyDefinition UsernameRequiredKey =
    ValidationKeyDefinition.Create("user.username.required");

private static readonly ValidationKeyDefinition EmailFormatKey =
    ValidationKeyDefinition.Create("user.email.format");
```

### 3. Fail Fast vs. Collect All

The pipeline collects all validation messages by default. To stop on the first failure, use the `ShortCircuit` property:

```c#
// Pipeline-level short-circuit
var pipeline = new ValidationPipeline<User> { ShortCircuit = true }
    .AddRule(user => ValidateBasicInfo(user))
    .AddRule(user => PerformExpensiveChecks(user));  // skipped if basic fails
```

For individual rule-level short-circuiting within a single rule function:

```c#
private static Result ExpensiveValidation(User user)
{
    var basicResult = ValidateBasicInfo(user);
    if (basicResult.IsFailure)
        return basicResult;

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


