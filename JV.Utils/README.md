# JV.Utils - Type-Safe Translation Keys

## Overview

This package provides a type-safe approach to handling translation keys and their parameters. The new `TranslationKeyDefinition` class ensures that users provide the correct number and types of parameters when creating validation messages.

Results are binary - they are either successful (contain no validation messages) or unsuccessful (contain one or more validation messages).

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
var validationMessage = ValidationMessage.CreateError(nameKey, "John", 3);

// This would throw an exception at runtime since the parameters don't match
// ValidationMessage.CreateError(nameKey, 3, "John"); // Invalid parameter types
// ValidationMessage.CreateError(nameKey, "John"); // Missing parameter
```

### Legacy Support

The old methods are still available but marked as obsolete:

```csharp
// These methods still work but are marked as obsolete
var legacyError = ValidationMessage.CreateError("legacy.key", "param1", "param2");
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
var failedUserResult = Result<User>.Error("user.not.found"); // Unsuccessful with typed context
```

## Best Practices

1. Define translation keys in a central location for reuse
2. Use meaningful parameter names that match your translation system
3. Consider creating constants for commonly used translation keys
4. Take advantage of the type safety to catch parameter errors early
5. Use `Result.Ok()` for successful operations without return values
6. Use `Result.Ok(value)` for successful operations with return values
7. Use `Result.Error()` to indicate validation failures
