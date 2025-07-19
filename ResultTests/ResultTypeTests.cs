using JV.Utils;
using JV.Utils.Extensions;
using JV.Utils.ValidationMessage;

namespace ResultTests;

public class ResultTypeTests
{
    /// <summary>
    /// Validates that the IsSuccessful property returns true for a Result that contains no validation errors.
    /// This test ensures the success state detection works correctly for error-free results.
    /// </summary>
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

    /// <summary>
    /// Validates that the IsSuccessful property returns false for a Result that contains validation errors.
    /// This test ensures the success state detection correctly identifies failed results.
    /// </summary>
    [Fact]
    public void IsSuccessful_WithErrors_ReturnsFalse()
    {
        // Arrange
        var errorKey = ValidationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("message");
        var message = ValidationMessage.Create(errorKey, "Some error");
        var result = Result.Create(new[] { message });

        // Act
        var isSuccessful = result.IsSuccessful;

        // Assert
        Assert.False(isSuccessful);
    }

    /// <summary>
    /// Validates that the IsFailure property returns true for a Result that contains validation errors.
    /// This test ensures the failure state detection works correctly and is the inverse of IsSuccessful.
    /// </summary>
    [Fact]
    public void IsFailure_WithErrors_ReturnsTrue()
    {
        // Arrange
        var errorKey = ValidationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("message");
        var message = ValidationMessage.Create(errorKey, "Some error");
        var result = Result.Create(new[] { message });

        // Act
        var isFailure = result.IsFailure;

        // Assert
        Assert.True(isFailure);
    }

    /// <summary>
    /// Validates that the ToString method returns a formatted string representation of the Result
    /// that includes the translation key for debugging and logging purposes.
    /// This test ensures basic string representation works without parameter expansion.
    /// </summary>
    [Fact]
    public void ToString_ReturnsFormattedMessage()
    {
        // Arrange
        var errorKey = ValidationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("message");
        var message = ValidationMessage.Create(errorKey, "Some error");
        var result = Result.Create(new[] { message });

        // Act
        var toString = result.ToString();

        // Assert
        Assert.Contains("error.key", toString);
    }

    /// <summary>
    /// Validates that the ToStringWithParameters method returns a formatted string representation 
    /// that includes both the translation key and the expanded parameter values.
    /// This test ensures detailed string representation works for comprehensive error reporting.
    /// </summary>
    [Fact]
    public void ToStringWithParameters_ReturnsFormattedMessageWithParameters()
    {
        // Arrange
        var errorKey = ValidationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("message");
        var message = ValidationMessage.Create(errorKey, "Some error");
        var result = Result.Create(new[] { message });

        // Act
        var toString = result.ToStringWithParameters();

        // Assert
        Assert.Contains("Error Key", toString);
        Assert.Contains("Some error", toString);
    }

    /// <summary>
    /// Validates that Result&lt;T&gt; properly stores and retrieves the value when created successfully.
    /// This test ensures the generic Result type correctly handles value storage for successful operations.
    /// </summary>
    [Fact]
    public void ResultOfT_WithValue_StoresValue()
    {
        // Arrange & Act
        var result = Result.Ok("test value");

        // Assert
        Assert.Equal("test value", result.Value);
    }

    /// <summary>
    /// Validates that the Merge operation correctly combines validation messages from multiple Result instances.
    /// This test ensures that when merging results, all validation messages are preserved and combined properly,
    /// which is essential for collecting multiple validation errors across different operations.
    /// </summary>
    [Fact]
    public void ResultOfT_Merge_CombinesValidationMessages()
    {
        // Arrange
        var errorKey1 = ValidationKeyDefinition.Create("error.key1", "Error Key 1")
            .WithStringParameter("message");
        var errorKey2 = ValidationKeyDefinition.Create("error.key2", "Error Key 2")
            .WithStringParameter("message");

        var result1 = Result.Ok("test value").Merge(
            Result.Create(new[] { ValidationMessage.Create(errorKey1, "Error 1") }));
        var result2 = Result.Create(new[] { ValidationMessage.Create(errorKey2, "Error 2") });

        // Act
        var mergedResult = result1.Merge(result2);

        // Assert
        Assert.Equal(2, mergedResult.ValidationMessages.Count());
        Assert.Contains(mergedResult.ValidationMessages, m => m.Parameters[0] == "Error 1");
        Assert.Contains(mergedResult.ValidationMessages, m => m.Parameters[0] == "Error 2");
    }
}