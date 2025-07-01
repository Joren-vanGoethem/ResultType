using JV.Utils;

namespace ResultTests;

public class TranslationParameterTests
{
  [Fact]
  public void StringParameter_ValidatesCorrectly()
  {
    // Arrange
    var parameter = new TranslationParameter("name", ParameterType.String);

    // Act & Assert
    Assert.True(parameter.ValidateValue("test"));
    Assert.False(parameter.ValidateValue(123));
  }

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

  [Fact]
  public void EnumParameter_ValidatesCorrectly()
  {
    // Arrange
    var parameter = new TranslationParameter("severity", ParameterType.Enum);

    // Act & Assert

    Assert.False(parameter.ValidateValue("not an enum"));
  }

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

  [Fact]
  public void EmailParameter_ValidatesCorrectly()
  {
    // Arrange
    var parameter = new TranslationParameter("email", ParameterType.Email);

    // Act & Assert
    Assert.True(parameter.ValidateValue("test@example.com"));
    Assert.False(parameter.ValidateValue("not an email"));
  }

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

  [Fact]
  public void FormatValue_WithInvalidValue_ThrowsArgumentException()
  {
    // Arrange
    var parameter = new TranslationParameter("count", ParameterType.Integer);

    // Act & Assert
    Assert.Throws<ArgumentException>(() => parameter.FormatValue("not an integer"));
  }
}