using System;
using System.Collections.Generic;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;
using Xunit;

namespace ResultTests;

public class ValidationParameterExhaustiveTests
{
    [Fact]
    public void PositionalParameters_Array_ValidatesAndFormatsCorrectly()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age");

        var parameters = new object[] { "John", 30 };

        // Act
        var isValid = keyDefinition.ValidateParameters(parameters);
        var formatted = keyDefinition.FormatParameters(parameters);

        // Assert
        Assert.True(isValid);
        Assert.Equal("John", formatted[0]);
        Assert.Equal("30", formatted[1]);
    }

    [Fact]
    public void PositionalParameters_IEnumerable_ValidatesAndFormatsCorrectly()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age");

        var parameters = new List<object> { "John", 30 };

        // Act
        var isValid = keyDefinition.ValidateParameters(parameters);
        var formatted = keyDefinition.FormatParameters(parameters);

        // Assert
        Assert.True(isValid);
        Assert.Equal("John", formatted[0]);
        Assert.Equal("30", formatted[1]);
    }

    [Fact]
    public void NamedParameters_Dictionary_ValidatesAndFormatsCorrectly()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age");

        var parameters = new Dictionary<string, object>
        {
            { "name", "John" },
            { "age", 30 }
        };

        // Act
        var isValid = keyDefinition.ValidateParameters(parameters);
        var formatted = keyDefinition.FormatParameters(parameters);

        // Assert
        Assert.True(isValid);
        Assert.Equal("John", formatted[0]);
        Assert.Equal("30", formatted[1]);
    }

    [Fact]
    public void NamedParameters_AnonymousObject_ValidatesAndFormatsCorrectly()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age");

        var parameters = new { name = "John", age = 30 };

        // Act
        var isValid = keyDefinition.ValidateParameters(parameters);
        var formatted = keyDefinition.FormatParameters(parameters);

        // Assert
        Assert.True(isValid);
        Assert.Equal("John", formatted[0]);
        Assert.Equal("30", formatted[1]);
    }

    [Fact]
    public void DefaultValues_NamedParameters_MissingValue_UsesDefault()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age", 30);

        var parameters = new { name = "John" };

        // Act
        var isValid = keyDefinition.ValidateParameters(parameters);
        var formatted = keyDefinition.FormatParameters(parameters);

        // Assert
        Assert.True(isValid);
        Assert.Equal("John", formatted[0]);
        Assert.Equal("30", formatted[1]);
    }

    [Fact]
    public void DefaultValues_NamedParameters_ProvidedValue_OverridesDefault()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age", 30);

        var parameters = new { name = "John", age = 25 };

        // Act
        var isValid = keyDefinition.ValidateParameters(parameters);
        var formatted = keyDefinition.FormatParameters(parameters);

        // Assert
        Assert.True(isValid);
        Assert.Equal("John", formatted[0]);
        Assert.Equal("25", formatted[1]);
    }

    [Fact]
    public void DefaultValues_PositionalParameters_MissingValues_IsInvalidIfAmbiguous()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age", 30);

        // Positional parameters must match the count of definition parameters
        var parameters = new object[] { "John" };

        // Act
        var isValid = keyDefinition.ValidateParameters(parameters);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void SingleValue_OneParameterDefined_ValidatesAndFormatsCorrectly()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name");

        var parameter = "John";

        // Act
        var isValid = keyDefinition.ValidateParameters(parameter);
        var formatted = keyDefinition.FormatParameters(parameter);

        // Assert
        Assert.True(isValid);
        Assert.Equal("John", formatted[0]);
    }

    [Fact]
    public void SingleValue_MultipleParametersDefinedWithDefaults_ValidatesAndFormatsCorrectly()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age", 30);

        // "John" should match the first parameter because it's the only one without a default value
        // Wait, looking at the code:
        // if (Parameters.Count(p => p.DefaultValue is null) != 1) return false;
        // return Parameters[0].ValidateValue(parameters);
        
        var parameter = "John";

        // Act
        var isValid = keyDefinition.ValidateParameters(parameter);
        var formatted = keyDefinition.FormatParameters(parameter);

        // Assert
        Assert.True(isValid);
        Assert.Equal("John", formatted[0]);
        Assert.Equal("30", formatted[1]);
    }

    [Fact]
    public void SingleValue_Ambiguous_ReturnsFalse()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithStringParameter("city"); // Both don't have defaults

        var parameter = "John";

        // Act
        var isValid = keyDefinition.ValidateParameters(parameter);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidationMessage_Create_WithAnonymousObject_FormatsCorrectly()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test.key", "Hello {0}, you are {1} years old")
            .WithStringParameter("name")
            .WithIntParameter("age");

        // Act
        var message = ValidationMessage.Create(keyDefinition, new { name = "John", age = 30 });

        // Assert
        Assert.Equal("John", message.Parameters[0]);
        Assert.Equal("30", message.Parameters[1]);
    }

    [Fact]
    public void ValidationMessage_Create_WithDefaults_FormatsCorrectly()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test.key", "Hello {0}")
            .WithStringParameter("name", "Guest");

        // Act
        var message = ValidationMessage.Create(keyDefinition, null);

        // Assert
        Assert.Equal("Guest", message.Parameters[0]);
    }

    [Fact]
    public void NullParameters_AllHaveDefaults_ReturnsTrue()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name", "Default")
            .WithIntParameter("age", 30);

        // Act
        var isValid = keyDefinition.ValidateParameters(null);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void NullParameters_SomeMissingDefaults_ReturnsFalse()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name") // No default
            .WithIntParameter("age", 30);

        // Act
        var isValid = keyDefinition.ValidateParameters(null);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void TypeMismatch_Dictionary_ReturnsFalse()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithIntParameter("age");

        var parameters = new Dictionary<string, object> { { "age", "not an int" } };

        // Act
        var isValid = keyDefinition.ValidateParameters(parameters);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void TypeMismatch_AnonymousObject_ReturnsFalse()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithIntParameter("age");

        var parameters = new { age = "not an int" };

        // Act
        var isValid = keyDefinition.ValidateParameters(parameters);

        // Assert
        Assert.False(isValid);
    }
}
