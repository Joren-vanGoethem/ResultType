using JV.Utils;
using JV.Utils.Extensions;

namespace ResultTests;

public class ValidationMessageTests
{
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


  [Fact]
  public void CreateError_WithInvalidParameters_ThrowsArgumentException()
  {
    // Arrange
    var keyDefinition = TranslationKeyDefinition.Create("error.key", "Error Key")
      .WithStringParameter("name")
      .WithIntParameter("count");

    // Act & Assert
    Assert.Throws<ArgumentException>(() => ValidationMessage.CreateError(keyDefinition, 5, "John")); // Swapped types
  }

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