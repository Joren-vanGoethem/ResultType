using JV.Utils;
using JV.Utils.Extensions;

namespace ResultTests;

public class ValidationMessageTests
{
    /// <summary>
    /// Validates that TranslationKeyDefinition.ValidateParameters returns true when provided with 
    /// parameters that match the expected types and count defined in the key definition.
    /// This test ensures the parameter validation works correctly for valid input.
    /// </summary>
    [Fact]
    public void TranslationKeyDefinition_ValidateParameters_WithCorrectParameters_ReturnsTrue()
    {
        // Arrange
        var keyDefinition = TranslationKeyDefinition.Create("test.key", "Test Key")
            .WithStringParameter("name")
            .WithIntParameter("age");

        // Act
        var isValid = keyDefinition.ValidateParameters(new object[] { "John", 30 });

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Validates that TranslationKeyDefinition.ValidateParameters returns false when provided with 
    /// parameters that have incorrect types (even if the count is correct).
    /// This test ensures type safety in parameter validation by rejecting mismatched parameter types.
    /// </summary>
    [Fact]
    public void TranslationKeyDefinition_ValidateParameters_WithIncorrectParameterTypes_ReturnsFalse()
    {
        // Arrange
        var keyDefinition = TranslationKeyDefinition.Create("test.key", "Test Key")
            .WithStringParameter("name")
            .WithIntParameter("age");

        // Act
        var isValid = keyDefinition.ValidateParameters(new object[] { 30, "John" }); // Swapped types

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Validates that TranslationKeyDefinition.ValidateParameters returns false when provided with 
    /// an incorrect number of parameters (fewer than expected).
    /// This test ensures parameter count validation works correctly by rejecting incomplete parameter sets.
    /// </summary>
    [Fact]
    public void TranslationKeyDefinition_ValidateParameters_WithIncorrectParameterCount_ReturnsFalse()
    {
        // Arrange
        var keyDefinition = TranslationKeyDefinition.Create("test.key", "Test Key")
            .WithStringParameter("name")
            .WithIntParameter("age");

        // Act
        var isValid = keyDefinition.ValidateParameters(new object[] { "John" }); // Missing parameter

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Validates that ValidationMessage.CreateError successfully creates a ValidationMessage when 
    /// provided with valid parameters that match the key definition.
    /// This test ensures the error message creation process works correctly and stores parameters properly.
    /// </summary>
    [Fact]
    public void CreateError_WithValidParameters_CreatesValidationMessage()
    {
        // Arrange
        var keyDefinition = TranslationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("name")
            .WithIntParameter("count");

        // Act
        var message = ValidationMessage.CreateError(keyDefinition, "John", 5);

        // Assert
        Assert.Equal("Error Key", message.TranslationKey);
        Assert.Equal(2, message.Parameters.Length);
        Assert.Equal("John", message.Parameters[0]);
        Assert.Equal("5", message.Parameters[1]);
    }

    /// <summary>
    /// Validates that ValidationMessage.CreateError throws an ArgumentException when provided with 
    /// parameters that don't match the expected types in the key definition.
    /// This test ensures parameter validation is enforced during error message creation to prevent runtime errors.
    /// </summary>
    [Fact]
    public void CreateError_WithInvalidParameters_ThrowsArgumentException()
    {
        // Arrange
        var keyDefinition = TranslationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("name")
            .WithIntParameter("count");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ValidationMessage.CreateError(keyDefinition, 5, "John")); // Swapped types
    }

    /// <summary>
    /// Validates that ValidationMessage.MapToErrorMessage produces a formatted string representation 
    /// that includes both the translation key and the parameter values.
    /// This test ensures error messages can be properly formatted for display to users or logging.
    /// </summary>
    [Fact]
    public void MapToErrorMessage_ReturnsFormattedMessage()
    {
        // Arrange
        var keyDefinition = TranslationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("name")
            .WithIntParameter("count");
        var message = ValidationMessage.CreateError(keyDefinition, "John", 5);

        // Act
        var errorMessage = message.MapToErrorMessage();

        // Assert
        Assert.Contains("Error Key", errorMessage);
        Assert.Contains("John", errorMessage);
        Assert.Contains("5", errorMessage);
    }
}