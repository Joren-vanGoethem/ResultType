using JV.Utils;
using JV.Utils.Extensions;

namespace ResultTests;

public class ResultTransformationExtensionsTests
{
  private readonly TranslationKeyDefinition _errorKey = TranslationKeyDefinition
    .Create("error.test", "Test Error")
    .WithStringParameter("message");
  
  private readonly TranslationKeyDefinition _errorNegativeKey = TranslationKeyDefinition
    .Create("error.negative")
    .WithStringParameter("message");
  
  private readonly TranslationKeyDefinition _errorTooSmallKey = TranslationKeyDefinition
    .Create("error.too.small")
    .WithStringParameter("message");

  #region Map Tests

  [Fact]
  public void Map_WithSuccessfulResult_MapsValueCorrectly()
  {
    // Arrange
    var successResult = Result.Ok(10);

    // Act
    var result = successResult.Map(value => value * 2);

    // Assert
    Assert.True(result.IsSuccessful);
    Assert.Equal(20, result.Value);
    Assert.Empty(result.ValidationMessages);
  }

  [Fact]
  public void Map_WithFailedResult_PreservesErrors()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Original error");
    var failedResult = Result.Create<int>(default, new[] { errorMessage });

    // Act
    var result = failedResult.Map(value => value * 2);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Single(result.ValidationMessages);
    Assert.Equal("Original error", result.ValidationMessages.First().Parameters[0]);
  }

  [Fact]
  public void Map_WithSuccessfulResult_CanMapToDifferentType()
  {
    // Arrange
    var successResult = Result.Ok(42);

    // Act
    var result = successResult.Map(value => $"Number: {value}");

    // Assert
    Assert.True(result.IsSuccessful);
    Assert.Equal("Number: 42", result.Value);
  }

  [Fact]
  public void Map_WhenMapperThrows_PropagatesException()
  {
    // Arrange
    var successResult = Result.Ok(10);

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
      successResult.Map<int, int>(value => throw new InvalidOperationException("Mapper failed")));
  }

  #endregion

  #region Bind Tests

  [Fact]
  public void Bind_WithSuccessfulResult_ExecutesBinder()
  {
    // Arrange
    var successResult = Result.Ok(5);

    // Act
    var result = successResult.Bind(value =>
      value > 0 ? Result.Ok(value * 2) : Result.Error(_errorNegativeKey, "Value is negative"));

    // Assert
    Assert.True(result.IsSuccessful);
    Assert.Equal(10, result.Value);
    Assert.Empty(result.ValidationMessages);
  }

  [Fact]
  public void Bind_WithFailedResult_DoesNotExecuteBinder()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Original error");
    var failedResult = Result.Create<int>(default, new[] { errorMessage });
    var binderExecuted = false;

    // Act
    var result = failedResult.Bind(value =>
    {
      binderExecuted = true;
      return Result.Ok(value * 2);
    });

    // Assert
    Assert.False(binderExecuted);
    Assert.True(result.IsFailure);
    Assert.Single(result.ValidationMessages);
    Assert.Equal("Original error", result.ValidationMessages.First().Parameters[0]);
  }

  [Fact]
  public void Bind_WithSuccessfulResultButBinderFails_ReturnsBinderError()
  {
    // Arrange
    var successResult = Result.Ok(-5);

    // Act
    var result = successResult.Bind(value =>
      value > 0 ? Result.Ok(value * 2) : Result.Error(_errorNegativeKey, "Value is negative"));

    // Assert
    Assert.True(result.IsFailure);
    Assert.Single(result.ValidationMessages);
  }

  [Fact]
  public void Bind_WhenBinderThrows_PropagatesException()
  {
    // Arrange
    var successResult = Result.Ok(10);

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
      successResult.Bind<int, int>(value => throw new InvalidOperationException("Binder failed")));
  }

  #endregion

  #region Match Tests

  [Fact]
  public void Match_WithSuccessfulResult_CallsOnSuccess()
  {
    // Arrange
    var successResult = Result.Ok("test");
    var onSuccessCalled = false;
    var onFailureCalled = false;

    // Act
    var result = successResult.Match(
      onSuccess: value =>
      {
        onSuccessCalled = true;
        return $"Success: {value}";
      },
      onFailure: errors =>
      {
        onFailureCalled = true;
        return "Failure";
      });

    // Assert
    Assert.True(onSuccessCalled);
    Assert.False(onFailureCalled);
    Assert.Equal("Success: test", result);
  }

  [Fact]
  public void Match_WithFailedResult_CallsOnFailure()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Test error");
    var failedResult = Result.Create<string>(default, new[] { errorMessage });
    var onSuccessCalled = false;
    var onFailureCalled = false;

    // Act
    var result = failedResult.Match(
      onSuccess: value =>
      {
        onSuccessCalled = true;
        return $"Success: {value}";
      },
      onFailure: errors =>
      {
        onFailureCalled = true;
        return $"Failure: {errors.First().Parameters[0]}";
      });

    // Assert
    Assert.False(onSuccessCalled);
    Assert.True(onFailureCalled);
    Assert.Equal("Failure: Test error", result);
  }

  [Fact]
  public void Match_WhenOnSuccessThrows_PropagatesException()
  {
    // Arrange
    var successResult = Result.Ok("test");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
      successResult.Match<string, string>(
        onSuccess: value => throw new InvalidOperationException("OnSuccess failed"),
        onFailure: errors => "Failure"));
  }

  [Fact]
  public void Match_WhenOnFailureThrows_PropagatesException()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Test error");
    var failedResult = Result.Create<string>(default, new[] { errorMessage });

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
      failedResult.Match<string, string>(
        onSuccess: value => "Success",
        onFailure: errors => throw new InvalidOperationException("OnFailure failed")));
  }

  #endregion

  #region Integration Tests

  [Fact]
  public void TransformationExtensions_CanBeChainedTogether()
  {
    // Arrange
    var initialResult = Result.Ok(5);

    // Act
    var finalResult = initialResult
      .Map(x => x * 2)
      .Bind(x => x > 5 ? Result.Ok(x + 10) : Result.Error(_errorTooSmallKey, "Value too small"))
      .Match(
        onSuccess: value => $"Final value: {value}",
        onFailure: errors => "Processing failed");

    // Assert
    Assert.Equal("Final value: 20", finalResult);
  }

  [Fact]
  public void TransformationExtensions_WithFailureInChain_StopsProcessing()
  {
    // Arrange
    var initialResult = Result.Ok(2);
    var mapperCalled = false;
    var matchOnSuccessCalled = false;

    // Act
    var finalResult = initialResult
      .Map(x =>
      {
        mapperCalled = true;
        return x * 2;
      })
      .Bind(x => x > 5 ? Result.Ok(x + 10) : Result.Error(_errorTooSmallKey, "Value too small"))
      .Match(
        onSuccess: value =>
        {
          matchOnSuccessCalled = true;
          return $"Final value: {value}";
        },
        onFailure: errors => $"Processing failed: {errors.First().Parameters[0]}");

    // Assert
    Assert.True(mapperCalled);
    Assert.False(matchOnSuccessCalled);
    Assert.Equal("Processing failed: Value too small", finalResult);
  }

  #endregion
}