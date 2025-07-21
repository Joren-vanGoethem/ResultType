using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests;

public class TranslationKeyBuilderExtensionsTests
{
    /// <summary>
    /// Validates that the WithStringParameter extension method correctly adds a string parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for string parameters.
    /// </summary>
    [Fact]
    public void WithStringParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithStringParameter("name");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("name", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.String, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithIntParameter extension method correctly adds an integer parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for integer parameters.
    /// </summary>
    [Fact]
    public void WithIntParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithIntParameter("count");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("count", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Integer, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithDecimalParameter extension method correctly adds a decimal parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for decimal parameters.
    /// </summary>
    [Fact]
    public void WithDecimalParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithDecimalParameter("price");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("price", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Decimal, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithDateTimeParameter extension method correctly adds a DateTime parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for DateTime parameters.
    /// </summary>
    [Fact]
    public void WithDateTimeParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithDateTimeParameter("created");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("created", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.DateTime, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithTimeOnlyParameter extension method correctly adds a TimeOnly parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for TimeOnly parameters.
    /// </summary>
    [Fact]
    public void WithTimeOnlyParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithTimeOnlyParameter("startTime");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("startTime", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.TimeOnly, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithDateOnlyParameter extension method correctly adds a DateOnly parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for DateOnly parameters.
    /// </summary>
    [Fact]
    public void WithDateOnlyParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithDateOnlyParameter("birthDate");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("birthDate", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.DateOnly, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithBooleanParameter extension method correctly adds a boolean parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for boolean parameters.
    /// </summary>
    [Fact]
    public void WithBooleanParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithBooleanParameter("isActive");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("isActive", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Boolean, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithGuidParameter extension method correctly adds a GUID parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for GUID parameters.
    /// </summary>
    [Fact]
    public void WithGuidParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithGuidParameter("id");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("id", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Guid, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithEnumParameter extension method correctly adds an enum parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for enumeration parameters.
    /// </summary>
    [Fact]
    public void WithEnumParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithEnumParameter("status");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("status", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Enum, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithUriParameter extension method correctly adds a URI parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for URI parameters.
    /// </summary>
    [Fact]
    public void WithUriParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithUriParameter("website");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("website", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Uri, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithTimeSpanParameter extension method correctly adds a TimeSpan parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for TimeSpan parameters.
    /// </summary>
    [Fact]
    public void WithTimeSpanParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithTimeSpanParameter("duration");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("duration", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.TimeSpan, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithEmailParameter extension method correctly adds an email parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for email parameters.
    /// </summary>
    [Fact]
    public void WithEmailParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithEmailParameter("email");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("email", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Email, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that the WithPhoneNumberParameter extension method correctly adds a phone number parameter 
    /// to a TranslationKeyDefinition with the proper name and type.
    /// This test ensures the fluent API for building translation keys works correctly for phone number parameters.
    /// </summary>
    [Fact]
    public void WithPhoneNumberParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithPhoneNumberParameter("phone");

        // Assert
        Assert.Single(keyWithParam.Parameters);
        Assert.Equal("phone", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.PhoneNumber, keyWithParam.Parameters[0].Type);
    }

    /// <summary>
    /// Validates that multiple parameter extension methods can be chained together to create 
    /// a TranslationKeyDefinition with multiple parameters of different types.
    /// This test ensures the fluent API supports method chaining and correctly maintains 
    /// parameter order and type information across multiple chained calls.
    /// </summary>
    [Fact]
    public void ChainedExtensions_AddMultipleParameters()
    {
        // Arrange
        var baseKey = ValidationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParams = baseKey
            .WithStringParameter("name")
            .WithIntParameter("age")
            .WithEmailParameter("email")
            .WithPhoneNumberParameter("phone");

        // Assert
        Assert.Equal(4, keyWithParams.Parameters.Count);
        Assert.Equal("name", keyWithParams.Parameters[0].Name);
        Assert.Equal("age", keyWithParams.Parameters[1].Name);
        Assert.Equal("email", keyWithParams.Parameters[2].Name);
        Assert.Equal("phone", keyWithParams.Parameters[3].Name);

        Assert.Equal(ParameterType.String, keyWithParams.Parameters[0].Type);
        Assert.Equal(ParameterType.Integer, keyWithParams.Parameters[1].Type);
        Assert.Equal(ParameterType.Email, keyWithParams.Parameters[2].Type);
        Assert.Equal(ParameterType.PhoneNumber, keyWithParams.Parameters[3].Type);
    }
}