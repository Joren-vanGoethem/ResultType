using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests;

public class ResultImplicitConversionTests
{
    [Fact]
    public void ImplicitConversion_FromSuccessfulResultToResultOfT_ThrowsNotSupportedException()
    {
        // Arrange
        var result = Result.Ok();

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() =>
        {
            Result<string> _ = result;
        });
        Assert.Contains("Cannot implicitly convert empty result to successful result with value", exception.Message);
    }

    [Fact]
    public void ImplicitConversion_FromFailedResultToResultOfT_Succeeds()
    {
        // Arrange
        var errorKey = ValidationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("message");
        var message = ValidationMessage.Create(errorKey, "Some error");
        var result = Result.Create(new[] { message });

        // Act
        Result<string> converted = result;

        // Assert
        Assert.True(converted.IsFailure);
        Assert.Single(converted.ValidationMessages);
    }

    [Fact]
    public void ImplicitConversion_FromValueToResultOfT_CreatesSuccessfulResult()
    {
        // Act
        Result<string> result = "test value";

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal("test value", result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromValidationMessageToResultOfT_CreatesFailedResult()
    {
        // Arrange
        var errorKey = ValidationKeyDefinition.Create("error.key", "Error Key")
            .WithStringParameter("message");
        var message = ValidationMessage.Create(errorKey, "Some error");

        // Act
        Result<string> result = message;

        // Assert
        Assert.True(result.IsFailure);
        Assert.Single(result.ValidationMessages);
    }
}
