using JV.Utils;

namespace ResultTests;

public class TranslationParameterTests
{
    /// <summary>
    /// Validates that a TranslationParameter configured for string type correctly accepts valid string values
    /// and rejects non-string values like integers.
    /// This test ensures the basic type validation works for string parameters.
    /// </summary>
    [Fact]
    public void StringParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("name", ParameterType.String);

        // Act & Assert
        Assert.True(parameter.ValidateValue("test"));
        Assert.False(parameter.ValidateValue(123));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for integer type correctly accepts various numeric types
    /// (int, long, string representations of numbers) and rejects invalid values.
    /// This test ensures the integer parameter validation has flexible type conversion while maintaining type safety.
    /// </summary>
    [Fact]
    public void IntegerParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("count", ParameterType.Integer);

        // Act & Assert
        Assert.True(parameter.ValidateValue(123));
        Assert.True(parameter.ValidateValue(123L));
        Assert.True(parameter.ValidateValue("123"));
        Assert.False(parameter.ValidateValue("abc"));
        Assert.False(parameter.ValidateValue(123.45));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for decimal type correctly accepts various decimal types
    /// (double, float, decimal, string representations) and rejects non-numeric values.
    /// This test ensures the decimal parameter validation supports multiple numeric types while rejecting invalid input.
    /// </summary>
    [Fact]
    public void DecimalParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("price", ParameterType.Decimal);

        // Act & Assert
        Assert.True(parameter.ValidateValue(123.45));
        Assert.True(parameter.ValidateValue(123.45f));
        Assert.True(parameter.ValidateValue(123.45m));
        Assert.True(parameter.ValidateValue("123.45"));
        Assert.False(parameter.ValidateValue("abc"));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for DateTime type correctly accepts DateTime objects
    /// and valid date string representations while rejecting invalid date strings.
    /// This test ensures DateTime parameter validation supports both native DateTime objects and parseable strings.
    /// </summary>
    [Fact]
    public void DateTimeParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("date", ParameterType.DateTime);
        var dateTime = new DateTime(2025, 1, 1);

        // Act & Assert
        Assert.True(parameter.ValidateValue(dateTime));
        Assert.True(parameter.ValidateValue("2025-01-01"));
        Assert.False(parameter.ValidateValue("not a date"));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for TimeOnly type correctly accepts TimeOnly objects
    /// and valid time string representations while rejecting invalid time strings.
    /// This test ensures TimeOnly parameter validation supports both native TimeOnly objects and parseable strings.
    /// </summary>
    [Fact]
    public void TimeOnlyParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("time", ParameterType.TimeOnly);
        var timeOnly = new TimeOnly(14, 30);

        // Act & Assert
        Assert.True(parameter.ValidateValue(timeOnly));
        Assert.True(parameter.ValidateValue("14:30"));
        Assert.False(parameter.ValidateValue("not a time"));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for DateOnly type correctly accepts DateOnly objects
    /// and valid date string representations while rejecting invalid date strings.
    /// This test ensures DateOnly parameter validation supports both native DateOnly objects and parseable strings.
    /// </summary>
    [Fact]
    public void DateOnlyParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("date", ParameterType.DateOnly);
        var dateOnly = new DateOnly(2025, 1, 1);

        // Act & Assert
        Assert.True(parameter.ValidateValue(dateOnly));
        Assert.True(parameter.ValidateValue("2025-01-01"));
        Assert.False(parameter.ValidateValue("not a date"));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for boolean type correctly accepts boolean values
    /// and valid boolean string representations ("true", "false") while rejecting invalid strings.
    /// This test ensures boolean parameter validation supports both native boolean values and parseable strings.
    /// </summary>
    [Fact]
    public void BooleanParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("active", ParameterType.Boolean);

        // Act & Assert
        Assert.True(parameter.ValidateValue(true));
        Assert.True(parameter.ValidateValue(false));
        Assert.True(parameter.ValidateValue("true"));
        Assert.True(parameter.ValidateValue("false"));
        Assert.False(parameter.ValidateValue("not a bool"));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for GUID type correctly accepts Guid objects
    /// and valid GUID string representations while rejecting invalid GUID strings.
    /// This test ensures GUID parameter validation supports both native Guid objects and parseable strings.
    /// </summary>
    [Fact]
    public void GuidParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("id", ParameterType.Guid);
        var guid = Guid.NewGuid();

        // Act & Assert
        Assert.True(parameter.ValidateValue(guid));
        Assert.True(parameter.ValidateValue(guid.ToString()));
        Assert.False(parameter.ValidateValue("not a guid"));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for enum type correctly rejects non-enum values.
    /// This test ensures enum parameter validation works correctly by rejecting invalid string representations.
    /// Note: The test appears incomplete as it only tests rejection of invalid values, not acceptance of valid enums.
    /// </summary>
    [Fact]
    public void EnumParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("severity", ParameterType.Enum);

        // Act & Assert

        Assert.False(parameter.ValidateValue("not an enum"));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for URI type correctly accepts Uri objects
    /// and valid URI string representations while rejecting invalid URI strings.
    /// This test ensures URI parameter validation supports both native Uri objects and parseable strings.
    /// </summary>
    [Fact]
    public void UriParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("url", ParameterType.Uri);
        var uri = new Uri("https://example.com");

        // Act & Assert
        Assert.True(parameter.ValidateValue(uri));
        Assert.True(parameter.ValidateValue("https://example.com"));
        Assert.False(parameter.ValidateValue("not a url"));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for TimeSpan type correctly accepts TimeSpan objects
    /// and valid TimeSpan string representations while rejecting invalid TimeSpan strings.
    /// This test ensures TimeSpan parameter validation supports both native TimeSpan objects and parseable strings.
    /// </summary>
    [Fact]
    public void TimeSpanParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("duration", ParameterType.TimeSpan);
        var timeSpan = TimeSpan.FromHours(2);

        // Act & Assert
        Assert.True(parameter.ValidateValue(timeSpan));
        Assert.True(parameter.ValidateValue("02:00:00"));
        Assert.False(parameter.ValidateValue("not a timespan"));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for email type correctly accepts valid email addresses
    /// and rejects invalid email strings.
    /// This test ensures email parameter validation performs proper email format checking.
    /// </summary>
    [Fact]
    public void EmailParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("email", ParameterType.Email);

        // Act & Assert
        Assert.True(parameter.ValidateValue("test@example.com"));
        Assert.False(parameter.ValidateValue("not an email"));
    }

    /// <summary>
    /// Validates that a TranslationParameter configured for phone number type correctly accepts various
    /// phone number formats and rejects invalid phone number strings.
    /// This test ensures phone number parameter validation supports multiple common phone number formats.
    /// </summary>
    [Fact]
    public void PhoneNumberParameter_ValidatesCorrectly()
    {
        // Arrange
        var parameter = new TranslationParameter("phone", ParameterType.PhoneNumber);

        // Act & Assert
        Assert.True(parameter.ValidateValue("+1234567890"));
        Assert.True(parameter.ValidateValue("(123) 456-7890"));
        Assert.False(parameter.ValidateValue("not a phone"));
    }

    /// <summary>
    /// Validates that the FormatValue method correctly converts a valid value to its string representation
    /// for use in translation message formatting.
    /// This test ensures values can be properly formatted for display in localized messages.
    /// </summary>
    [Fact]
    public void FormatValue_WithValidValue_ReturnsFormattedString()
    {
        // Arrange
        var parameter = new TranslationParameter("count", ParameterType.Integer);

        // Act
        var formatted = parameter.FormatValue(123);

        // Assert
        Assert.Equal("123", formatted);
    }

    /// <summary>
    /// Validates that the FormatValue method throws an ArgumentException when attempting to format
    /// a value that doesn't match the parameter's expected type.
    /// This test ensures type safety is enforced during the formatting process to prevent runtime errors.
    /// </summary>
    [Fact]
    public void FormatValue_WithInvalidValue_ThrowsArgumentException()
    {
        // Arrange
        var parameter = new TranslationParameter("count", ParameterType.Integer);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parameter.FormatValue("not an integer"));
    }
}