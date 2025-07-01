using System;
using JV.Utils;
using JV.Utils.Extensions;
using Xunit;

namespace TestProject1;

public class TranslationKeyBuilderExtensionsTests
{
    [Fact]
    public void WithStringParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithStringParameter("name");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("name", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.String, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithIntParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithIntParameter("count");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("count", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Integer, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithDecimalParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithDecimalParameter("price");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("price", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Decimal, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithDateTimeParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithDateTimeParameter("created");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("created", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.DateTime, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithTimeOnlyParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithTimeOnlyParameter("startTime");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("startTime", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.TimeOnly, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithDateOnlyParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithDateOnlyParameter("birthDate");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("birthDate", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.DateOnly, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithBooleanParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithBooleanParameter("isActive");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("isActive", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Boolean, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithGuidParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithGuidParameter("id");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("id", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Guid, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithEnumParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithEnumParameter("status");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("status", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Enum, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithUriParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithUriParameter("website");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("website", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Uri, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithTimeSpanParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithTimeSpanParameter("duration");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("duration", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.TimeSpan, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithEmailParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithEmailParameter("email");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("email", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.Email, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void WithPhoneNumberParameter_AddsParameterCorrectly()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

        // Act
        var keyWithParam = baseKey.WithPhoneNumberParameter("phone");

        // Assert
        Assert.Equal(1, keyWithParam.Parameters.Count);
        Assert.Equal("phone", keyWithParam.Parameters[0].Name);
        Assert.Equal(ParameterType.PhoneNumber, keyWithParam.Parameters[0].Type);
    }

    [Fact]
    public void ChainedExtensions_AddMultipleParameters()
    {
        // Arrange
        var baseKey = TranslationKeyDefinition.Create("test.key", "Test Key");

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
