using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests;

public class ResultCastTests
{
    [Fact]
    public void Cast_OnSuccessfulResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result.Ok("test value");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => result.Cast<int>());
        Assert.Contains("Cannot cast a successful result to a different type", exception.Message);
    }

    [Fact]
    public void Cast_OnFailedResult_ReturnsResultWithSameValidationMessages()
    {
        // Arrange
        var errorKey = ValidationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("message");
        var message = ValidationMessage.Create(errorKey, "Some error");
        var result = Result.Create("unused", new[] { message });

        // Act
        var castResult = result.Cast<int>();

        // Assert
        Assert.True(castResult.IsFailure);
        Assert.Single(castResult.ValidationMessages);
        Assert.Equal("error.key", castResult.ValidationMessages.First().KeyDefinition!.Key);
    }

    [Fact]
    public void Cast_OnFailedResult_PreservesAllValidationMessages()
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

        // Act
        var castResult = result.Cast<int>();

        // Assert
        Assert.True(castResult.IsFailure);
        Assert.Equal(2, castResult.ValidationMessages.Count());
    }
}
