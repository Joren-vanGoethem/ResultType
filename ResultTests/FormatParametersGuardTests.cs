using System;
using System.Collections;
using System.Collections.Generic;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;
using Xunit;

namespace ResultTests;

public class FormatParametersGuardTests
{
    /// <summary>
    /// An object that implements IEnumerable (yielding mismatched count) but also has
    /// named properties matching the parameter definitions. ValidateParameters passes
    /// via anonymous object reflection, but FormatParameters enters the IEnumerable branch
    /// where the count mismatch leaves array slots unfilled.
    /// </summary>
    private class EnumerableWithNamedProperties : IEnumerable<object>
    {
        public string name { get; set; } = "John";
        public int age { get; set; } = 30;

        // Yield a different count than the 2 defined parameters
        public IEnumerator<object> GetEnumerator()
        {
            yield return "only-one-item";
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void FormatParameters_EnumerableWithNamedProperties_FormatsViaReflection()
    {
        // Arrange — object implements IEnumerable (mismatched count) but has named properties
        // matching the definition. Should fall through to anonymous object reflection.
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age");

        var parameters = new EnumerableWithNamedProperties();

        // Act
        var isValid = keyDefinition.ValidateParameters(parameters);
        Assert.True(isValid);

        var formatted = keyDefinition.FormatParameters(parameters);

        // Assert — should resolve via named properties, not the IEnumerable values
        Assert.Equal(2, formatted.Length);
        Assert.All(formatted, item => Assert.NotNull(item));
        Assert.Equal("John", formatted[0]);
        Assert.Equal("30", formatted[1]);
    }

    [Fact]
    public void FormatParameters_AllBranches_NeverReturnNullElements()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age");

        object[] inputs =
        [
            new object[] { "John", 30 },                                           // object[] branch
            new List<object> { "John", 30 },                                       // IEnumerable branch (count matches)
            new Dictionary<string, object> { { "name", "John" }, { "age", 30 } },  // IDictionary branch
            new { name = "John", age = 30 }                                        // anonymous object branch
        ];

        foreach (var input in inputs)
        {
            // Act
            var formatted = keyDefinition.FormatParameters(input);

            // Assert — no element should ever be null
            Assert.Equal(2, formatted.Length);
            Assert.All(formatted, item => Assert.NotNull(item));
        }
    }

    [Fact]
    public void FormatParameters_NullParameters_WithDefaults_NeverReturnNullElements()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name", "Guest")
            .WithIntParameter("age", 25);

        // Act
        var formatted = keyDefinition.FormatParameters(null);

        // Assert
        Assert.Equal(2, formatted.Length);
        Assert.All(formatted, item => Assert.NotNull(item));
        Assert.Equal("Guest", formatted[0]);
        Assert.Equal("25", formatted[1]);
    }

    [Fact]
    public void FormatParameters_SingleStringValue_WithDefaultsOnOtherParams_NeverReturnNullElements()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age", 30);

        // Act
        var formatted = keyDefinition.FormatParameters("John");

        // Assert
        Assert.Equal(2, formatted.Length);
        Assert.All(formatted, item => Assert.NotNull(item));
        Assert.Equal("John", formatted[0]);
        Assert.Equal("30", formatted[1]);
    }

    [Fact]
    public void FormatParameters_SingleDecimalValue_ElseBranch_NeverReturnNullElements()
    {
        // Decimal has reflection properties that could confuse the anonymous object branch,
        // so it exercises the single-value check in the else branch
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithDecimalParameter("amount");

        // Act
        var formatted = keyDefinition.FormatParameters(42.5m);

        // Assert
        Assert.Single(formatted);
        Assert.NotNull(formatted[0]);
        Assert.Equal("42.5", formatted[0]);
    }

    [Fact]
    public void FormatParameters_InvalidParameters_Throws_ArgumentException()
    {
        // Arrange
        var keyDefinition = ValidationKeyDefinition.Create("test", "Test")
            .WithStringParameter("name")
            .WithIntParameter("age");

        // Act & Assert — invalid params should throw on ValidateParameters check
        Assert.Throws<ArgumentException>(() => keyDefinition.FormatParameters(42));
    }
}
