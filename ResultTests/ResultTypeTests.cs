using JV.Utils;
using JV.Utils.Extensions;

namespace ResultTests;

public class ResultTypeTests
{
  [Fact]
  public void IsSuccessful_WithNoErrors_ReturnsTrue()
  {
    // Arrange
    var result = Result.Ok();

    // Act
    var isSuccessful = result.IsSuccessful;

    // Assert
    Assert.True(isSuccessful);
  }

  [Fact]
  public void IsSuccessful_WithErrors_ReturnsFalse()
  {
    // Arrange
    var errorKey = TranslationKeyDefinition.Create("error.key", "Error Key")
      .WithStringParameter("message");
    var message = ValidationMessage.CreateError(errorKey, "Some error");
    var result = Result.Create(new[] { message });

    // Act
    var isSuccessful = result.IsSuccessful;

    // Assert
    Assert.False(isSuccessful);
  }

  [Fact]
  public void IsFailure_WithErrors_ReturnsTrue()
  {
    // Arrange
    var errorKey = TranslationKeyDefinition.Create("error.key", "Error Key")
      .WithStringParameter("message");
    var message = ValidationMessage.CreateError(errorKey, "Some error");
    var result = Result.Create(new[] { message });

    // Act
    var isFailure = result.IsFailure;

    // Assert
    Assert.True(isFailure);
  }

  [Fact]
  public void ToString_ReturnsFormattedMessage()
  {
    // Arrange
    var errorKey = TranslationKeyDefinition.Create("error.key", "Error Key")
      .WithStringParameter("message");
    var message = ValidationMessage.CreateError(errorKey, "Some error");
    var result = Result.Create(new[] { message });

    // Act
    var toString = result.ToString();

    // Assert
    Assert.Contains("error.key", toString);
  }

  [Fact]
  public void ToStringWithParameters_ReturnsFormattedMessageWithParameters()
  {
    // Arrange
    var errorKey = TranslationKeyDefinition.Create("error.key", "Error Key")
      .WithStringParameter("message");
    var message = ValidationMessage.CreateError(errorKey, "Some error");
    var result = Result.Create(new[] { message });

    // Act
    var toString = result.ToStringWithParameters();

    // Assert
    Assert.Contains("Error Key", toString);
    Assert.Contains("Some error", toString);
  }

  [Fact]
  public void ResultOfT_WithValue_StoresValue()
  {
    // Arrange & Act
    var result = Result.Ok("test value");

    // Assert
    Assert.Equal("test value", result.Value);
  }

  [Fact]
  public void ResultOfT_Merge_CombinesValidationMessages()
  {
    // Arrange
    var errorKey1 = TranslationKeyDefinition.Create("error.key1", "Error Key 1")
      .WithStringParameter("message");
    var errorKey2 = TranslationKeyDefinition.Create("error.key2", "Error Key 2")
      .WithStringParameter("message");

    var result1 = Result.Ok("test value").Merge(
      Result.Create(new[] { ValidationMessage.CreateError(errorKey1, "Error 1") }));
    var result2 = Result.Create(new[] { ValidationMessage.CreateError(errorKey2, "Error 2") });

    // Act
    var mergedResult = result1.Merge(result2);

    // Assert
    Assert.Equal(2, mergedResult.ValidationMessages.Count());
    Assert.Contains(mergedResult.ValidationMessages, m => m.Parameters[0] == "Error 1");
    Assert.Contains(mergedResult.ValidationMessages, m => m.Parameters[0] == "Error 2");
  }
}