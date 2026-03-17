using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests;

public class ResultValueAccessTests
{
    [Fact]
    public void Value_OnSuccessfulResult_ReturnsValue()
    {
        // Arrange
        var result = Result.Ok("test value");

        // Act
        var value = result.Value;

        // Assert
        Assert.Equal("test value", value);
    }

    [Fact]
    public void Value_OnSuccessfulResultWithInt_ReturnsValue()
    {
        // Arrange
        var result = Result.Ok(42);

        // Act
        var value = result.Value;

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void Value_OnFailedResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var errorKey = ValidationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("message");
        var message = ValidationMessage.Create(errorKey, "Some error");
        var result = Result.Create("unused", new[] { message });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("Cannot access Value on a failed result", exception.Message);
    }

    [Fact]
    public void Value_OnFailedResultWithMultipleErrors_ThrowsInvalidOperationException()
    {
        // Arrange
        var errorKey1 = ValidationKeyDefinition.Create("error.key1", "Error Key 1")
            .WithStringParameter("message");
        var errorKey2 = ValidationKeyDefinition.Create("error.key2", "Error Key 2")
            .WithStringParameter("message");
        var result = Result.Create("unused", new[]
        {
            ValidationMessage.Create(errorKey1, "Error 1"),
            ValidationMessage.Create(errorKey2, "Error 2")
        });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Value_OnFailedResultCreatedViaErrorFactory_ThrowsInvalidOperationException()
    {
        // Arrange
        var errorKey = ValidationKeyDefinition.Create("error.key", "Error Key");
        var result = Result.Error<string>(errorKey);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("Cannot access Value on a failed result", exception.Message);
    }
}
