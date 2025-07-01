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

  /// <summary>
  /// Validates that the Map extension method correctly transforms the value of a successful Result
  /// using the provided mapper function while preserving the success state.
  /// This test ensures that successful results can be transformed to new values without affecting 
  /// their success status or validation messages.
  /// </summary>
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

  /// <summary>
  /// Validates that the Map extension method preserves validation errors from failed Results
  /// without executing the mapper function, maintaining the original error state.
  /// This test ensures that failed results short-circuit the transformation pipeline correctly.
  /// </summary>
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

  /// <summary>
  /// Validates that the Map extension method can transform a successful Result's value 
  /// to a completely different type while maintaining the success state.
  /// This test ensures that type transformation works correctly in the mapping pipeline.
  /// </summary>
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

  /// <summary>
  /// Validates that the Map extension method properly propagates exceptions thrown by the mapper function.
  /// This test ensures that exceptions in the transformation logic are not swallowed but bubble up 
  /// to the calling code for proper error handling.
  /// </summary>
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

  /// <summary>
  /// Validates that the Bind extension method correctly executes the binder function for successful Results
  /// and returns the result of the binder function, enabling monadic composition.
  /// This test ensures that successful results can be chained with other operations that return Results.
  /// </summary>
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

  /// <summary>
  /// Validates that the Bind extension method does not execute the binder function for failed Results
  /// and preserves the original validation errors, maintaining the failure state.
  /// This test ensures that failed results properly short-circuit the bind operation.
  /// </summary>
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

  /// <summary>
  /// Validates that the Bind extension method correctly handles the case where a successful Result
  /// is bound to a function that returns a failed Result, properly transitioning from success to failure.
  /// This test ensures that bind operations can introduce new validation errors based on business logic.
  /// </summary>
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

  /// <summary>
  /// Validates that the Bind extension method properly propagates exceptions thrown by the binder function.
  /// This test ensures that exceptions in the binding logic are not swallowed but bubble up 
  /// to the calling code for proper error handling.
  /// </summary>
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

  /// <summary>
  /// Validates that the Match extension method correctly calls the onSuccess function for successful Results
  /// and returns the result of the onSuccess function, enabling pattern matching on Result state.
  /// This test ensures that successful results are properly handled in the pattern matching pipeline.
  /// </summary>
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

  /// <summary>
  /// Validates that the Match extension method correctly calls the onFailure function for failed Results
  /// and returns the result of the onFailure function, enabling proper error handling in pattern matching.
  /// This test ensures that failed results are properly handled in the pattern matching pipeline.
  /// </summary>
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

  /// <summary>
  /// Validates that the Match extension method properly propagates exceptions thrown by the onSuccess function.
  /// This test ensures that exceptions in the success handling logic are not swallowed but bubble up 
  /// to the calling code for proper error handling.
  /// </summary>
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

  /// <summary>
  /// Validates that the Match extension method properly propagates exceptions thrown by the onFailure function.
  /// This test ensures that exceptions in the failure handling logic are not swallowed but bubble up 
  /// to the calling code for proper error handling.
  /// </summary>
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

  /// <summary>
  /// Validates that the transformation extension methods (Map, Bind, Match) can be chained together
  /// to create a complete functional pipeline for processing Results.
  /// This integration test demonstrates how the transformation methods work together in real-world scenarios
  /// where multiple operations need to be composed while maintaining proper error handling.
  /// </summary>
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

  /// <summary>
  /// Validates that when a failure occurs in a chained transformation pipeline, 
  /// subsequent operations are properly short-circuited and the failure is propagated correctly.
  /// This integration test ensures that the fail-fast behavior works correctly across 
  /// the entire transformation chain, preventing unnecessary processing after a failure.
  /// </summary>
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