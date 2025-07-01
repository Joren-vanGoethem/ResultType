using JV.Utils;
using JV.Utils.Extensions;

namespace ResultTests;

public class AsyncResultExtensionsTests
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

  #region MapAsync Tests

  /// <summary>
  /// Validates that the MapAsync extension method correctly transforms the value of a successful Task&lt;Result&lt;T&gt;&gt;
  /// using an async mapper function while preserving the success state.
  /// This test ensures that successful async results can be transformed to new values asynchronously 
  /// without affecting their success status or validation messages.
  /// </summary>
  [Fact]
  public async Task MapAsync_WithSuccessfulResult_MapsValueCorrectly()
  {
    // Arrange
    var successResult = Task.FromResult(Result.Ok(10));

    // Act
    var result = await successResult.MapAsync(async value =>
    {
      await Task.Delay(1); // Simulate async work
      return value * 2;
    });

    // Assert
    Assert.True(result.IsSuccessful);
    Assert.Equal(20, result.Value);
    Assert.Empty(result.ValidationMessages);
  }

  /// <summary>
  /// Validates that the MapAsync extension method preserves validation errors from failed Task&lt;Result&lt;T&gt;&gt;
  /// without executing the async mapper function, maintaining the original error state.
  /// This test ensures that failed async results properly short-circuit the transformation pipeline.
  /// </summary>
  [Fact]
  public async Task MapAsync_WithFailedResult_PreservesErrors()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Original error");
    var failedResult = Task.FromResult(Result.Create<int>(default, new[] { errorMessage }));

    // Act
    var result = await failedResult.MapAsync(async value =>
    {
      await Task.Delay(1);
      return value * 2;
    });

    // Assert
    Assert.True(result.IsFailure);
    Assert.Single(result.ValidationMessages);
    Assert.Equal("Original error", result.ValidationMessages.First().Parameters[0]);
  }

  /// <summary>
  /// Validates that the MapAsync extension method can transform a successful Task&lt;Result&lt;T&gt;&gt;'s value 
  /// to a completely different type asynchronously while maintaining the success state.
  /// This test ensures that async type transformation works correctly in the mapping pipeline.
  /// </summary>
  [Fact]
  public async Task MapAsync_WithSuccessfulResult_CanMapToDifferentType()
  {
    // Arrange
    var successResult = Task.FromResult(Result.Ok(42));

    // Act
    var result = await successResult.MapAsync(async value =>
    {
      await Task.Delay(1);
      return $"Number: {value}";
    });

    // Assert
    Assert.True(result.IsSuccessful);
    Assert.Equal("Number: 42", result.Value);
  }

  /// <summary>
  /// Validates that the MapAsync extension method properly propagates exceptions thrown by the async mapper function.
  /// This test ensures that exceptions in the async transformation logic are not swallowed but bubble up 
  /// to the calling code for proper error handling.
  /// </summary>
  [Fact]
  public async Task MapAsync_WhenMapperThrows_PropagatesException()
  {
    // Arrange
    var successResult = Task.FromResult(Result.Ok(10));

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      await successResult.MapAsync(async value =>
      {
        await Task.Delay(1);
        throw new InvalidOperationException("Mapper failed");
        return value; // just to help intellisense with type inference
      }));
  }

  #endregion

  #region BindAsync Tests

  /// <summary>
  /// Validates that the BindAsync extension method correctly executes the async binder function for successful Task&lt;Result&lt;T&gt;&gt;
  /// and returns the result of the binder function, enabling monadic composition with async operations.
  /// This test ensures that successful async results can be chained with other async operations that return Results.
  /// </summary>
  [Fact]
  public async Task BindAsync_WithSuccessfulResult_ExecutesBinder()
  {
    // Arrange
    var successResult = Task.FromResult(Result.Ok(5));

    // Act
    var result = await successResult.BindAsync(async value =>
    {
      await Task.Delay(1);
      return value > 0 ? Result.Ok(value * 2) : Result.Error(_errorNegativeKey, "Value is negative");
    });

    // Assert
    Assert.True(result.IsSuccessful);
    Assert.Equal(10, result.Value);
    Assert.Empty(result.ValidationMessages);
  }

  /// <summary>
  /// Validates that the BindAsync extension method does not execute the async binder function for failed Task&lt;Result&lt;T&gt;&gt;
  /// and preserves the original validation errors, maintaining the failure state.
  /// This test ensures that failed async results properly short-circuit the bind operation.
  /// </summary>
  [Fact]
  public async Task BindAsync_WithFailedResult_DoesNotExecuteBinder()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Original error");
    var failedResult = Task.FromResult(Result.Create<int>(default, new[] { errorMessage }));
    var binderExecuted = false;

    // Act
    var result = await failedResult.BindAsync(async value =>
    {
      binderExecuted = true;
      await Task.Delay(1);
      return Result.Ok(value * 2);
    });

    // Assert
    Assert.False(binderExecuted);
    Assert.True(result.IsFailure);
    Assert.Single(result.ValidationMessages);
    Assert.Equal("Original error", result.ValidationMessages.First().Parameters[0]);
  }

  /// <summary>
  /// Validates that the BindAsync extension method correctly handles the case where a successful Task&lt;Result&lt;T&gt;&gt;
  /// is bound to an async function that returns a failed Result, properly transitioning from success to failure.
  /// This test ensures that async bind operations can introduce new validation errors based on business logic.
  /// </summary>
  [Fact]
  public async Task BindAsync_WithSuccessfulResultButBinderFails_ReturnsBinderError()
  {
    // Arrange
    var successResult = Task.FromResult(Result.Ok(-5));

    // Act
    var result = await successResult.BindAsync(async value =>
    {
      await Task.Delay(1);
      return value > 0 ? Result.Ok(value * 2) : Result.Error(_errorNegativeKey, "Value is negative");
    });

    // Assert
    Assert.True(result.IsFailure);
    Assert.Single(result.ValidationMessages);
  }

  /// <summary>
  /// Validates that the BindAsync extension method properly propagates exceptions thrown by the async binder function.
  /// This test ensures that exceptions in the async binding logic are not swallowed but bubble up 
  /// to the calling code for proper error handling.
  /// </summary>
  [Fact]
  public async Task BindAsync_WhenBinderThrows_PropagatesException()
  {
    // Arrange
    var successResult = Task.FromResult(Result.Ok(10));

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      await successResult.BindAsync(async value =>
      {
        await Task.Delay(1);
        throw new InvalidOperationException("Binder failed");
        return Result.Create(value, []); // just to help intellisense with type inference
      }));
  }

  #endregion

  #region MatchAsync Tests

  /// <summary>
  /// Validates that the MatchAsync extension method correctly calls the async onSuccess function for successful Task&lt;Result&lt;T&gt;&gt;
  /// and returns the result of the onSuccess function, enabling async pattern matching on Result state.
  /// This test ensures that successful async results are properly handled in the async pattern matching pipeline.
  /// </summary>
  [Fact]
  public async Task MatchAsync_TaskResultWithAsyncHandlers_SuccessfulResult_CallsOnSuccess()
  {
    // Arrange
    var successResult = Task.FromResult(Result.Ok("test"));
    var onSuccessCalled = false;
    var onFailureCalled = false;

    // Act
    var result = await successResult.MatchAsync(
      onSuccess: async value =>
      {
        onSuccessCalled = true;
        await Task.Delay(1);
        return $"Success: {value}";
      },
      onFailure: async errors =>
      {
        onFailureCalled = true;
        await Task.Delay(1);
        return "Failure";
      });

    // Assert
    Assert.True(onSuccessCalled);
    Assert.False(onFailureCalled);
    Assert.Equal("Success: test", result);
  }

  /// <summary>
  /// Validates that the MatchAsync extension method correctly calls the async onFailure function for failed Task&lt;Result&lt;T&gt;&gt;
  /// and returns the result of the onFailure function, enabling proper async error handling in pattern matching.
  /// This test ensures that failed async results are properly handled in the async pattern matching pipeline.
  /// </summary>
  [Fact]
  public async Task MatchAsync_TaskResultWithAsyncHandlers_FailedResult_CallsOnFailure()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Test error");
    var failedResult = Task.FromResult(Result.Create<string>(default, new[] { errorMessage }));
    var onSuccessCalled = false;
    var onFailureCalled = false;

    // Act
    var result = await failedResult.MatchAsync(
      onSuccess: async value =>
      {
        onSuccessCalled = true;
        await Task.Delay(1);
        return $"Success: {value}";
      },
      onFailure: async errors =>
      {
        onFailureCalled = true;
        await Task.Delay(1);
        return $"Failure: {errors.First().Parameters[0]}";
      });

    // Assert
    Assert.False(onSuccessCalled);
    Assert.True(onFailureCalled);
    Assert.Equal("Failure: Test error", result);
  }

  /// <summary>
  /// Validates that the MatchAsync extension method correctly calls the synchronous onSuccess function for successful Task&lt;Result&lt;T&gt;&gt;
  /// when using sync handlers with async results, enabling mixed sync/async pattern matching.
  /// This test ensures compatibility between async Results and synchronous handler functions.
  /// </summary>
  [Fact]
  public async Task MatchAsync_TaskResultWithSyncHandlers_SuccessfulResult_CallsOnSuccess()
  {
    // Arrange
    var successResult = Task.FromResult(Result.Ok(42));
    var onSuccessCalled = false;
    var onFailureCalled = false;

    // Act
    var result = await successResult.MatchAsync(
      onSuccess: value =>
      {
        onSuccessCalled = true;
        return value * 2;
      },
      onFailure: errors =>
      {
        onFailureCalled = true;
        return -1;
      });

    // Assert
    Assert.True(onSuccessCalled);
    Assert.False(onFailureCalled);
    Assert.Equal(84, result);
  }

  /// <summary>
  /// Validates that the MatchAsync extension method correctly calls the synchronous onFailure function for failed Task&lt;Result&lt;T&gt;&gt;
  /// when using sync handlers with async results, enabling mixed sync/async error handling.
  /// This test ensures compatibility between async Results and synchronous error handler functions.
  /// </summary>
  [Fact]
  public async Task MatchAsync_TaskResultWithSyncHandlers_FailedResult_CallsOnFailure()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Test error");
    var failedResult = Task.FromResult(Result.Create<int>(default, new[] { errorMessage }));
    var onSuccessCalled = false;
    var onFailureCalled = false;

    // Act
    var result = await failedResult.MatchAsync(
      onSuccess: value =>
      {
        onSuccessCalled = true;
        return value * 2;
      },
      onFailure: errors =>
      {
        onFailureCalled = true;
        return errors.Count();
      });

    // Assert
    Assert.False(onSuccessCalled);
    Assert.True(onFailureCalled);
    Assert.Equal(1, result);
  }

  /// <summary>
  /// Validates that the MatchAsync extension method correctly calls the async onSuccess function for successful Result&lt;T&gt;
  /// when using async handlers with sync results, enabling async operations on synchronous Results.
  /// This test ensures that synchronous Results can be processed with async handler functions.
  /// </summary>
  [Fact]
  public async Task MatchAsync_ResultWithAsyncHandlers_SuccessfulResult_CallsOnSuccess()
  {
    // Arrange
    var successResult = Result.Ok("hello");
    var onSuccessCalled = false;
    var onFailureCalled = false;

    // Act
    var result = await successResult.MatchAsync(
      onSuccess: async value =>
      {
        onSuccessCalled = true;
        await Task.Delay(1);
        return value.ToUpper();
      },
      onFailure: async errors =>
      {
        onFailureCalled = true;
        await Task.Delay(1);
        return "ERROR";
      });

    // Assert
    Assert.True(onSuccessCalled);
    Assert.False(onFailureCalled);
    Assert.Equal("HELLO", result);
  }

  /// <summary>
  /// Validates that the MatchAsync extension method correctly calls the async onFailure function for failed Result&lt;T&gt;
  /// when using async handlers with sync results, enabling async error processing on synchronous Results.
  /// This test ensures that synchronous Results can be processed with async error handler functions.
  /// </summary>
  [Fact]
  public async Task MatchAsync_ResultWithAsyncHandlers_FailedResult_CallsOnFailure()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Test error");
    var failedResult = Result.Create<string>(default, new[] { errorMessage });
    var onSuccessCalled = false;
    var onFailureCalled = false;

    // Act
    var result = await failedResult.MatchAsync(
      onSuccess: async value =>
      {
        onSuccessCalled = true;
        await Task.Delay(1);
        return value.ToUpper();
      },
      onFailure: async errors =>
      {
        onFailureCalled = true;
        await Task.Delay(1);
        return $"ERROR: {errors.Count()}";
      });

    // Assert
    Assert.False(onSuccessCalled);
    Assert.True(onFailureCalled);
    Assert.Equal("ERROR: 1", result);
  }

  /// <summary>
  /// Validates that the MatchAsync extension method correctly calls the async onSuccess function for successful Task&lt;Result&gt;
  /// (non-generic Result), enabling async pattern matching on non-value Results.
  /// This test ensures that async Results without values are properly handled in pattern matching.
  /// </summary>
  [Fact]
  public async Task MatchAsync_TaskResult_SuccessfulResult_CallsOnSuccess()
  {
    // Arrange
    var successResult = Task.FromResult(Result.Ok());
    var onSuccessCalled = false;
    var onFailureCalled = false;

    // Act
    var result = await successResult.MatchAsync(
      onSuccess: async () =>
      {
        onSuccessCalled = true;
        await Task.Delay(1);
        return "Success";
      },
      onFailure: async errors =>
      {
        onFailureCalled = true;
        await Task.Delay(1);
        return $"Failure: {errors.Count()}";
      });

    // Assert
    Assert.True(onSuccessCalled);
    Assert.False(onFailureCalled);
    Assert.Equal("Success", result);
  }

  /// <summary>
  /// Validates that the MatchAsync extension method correctly calls the async onFailure function for failed Task&lt;Result&gt;
  /// (non-generic Result), enabling async error handling on non-value Results.
  /// This test ensures that failed async Results without values are properly handled in error scenarios.
  /// </summary>
  [Fact]
  public async Task MatchAsync_TaskResult_FailedResult_CallsOnFailure()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Test error");
    var failedResult = Task.FromResult(Result.Create(new[] { errorMessage }));
    var onSuccessCalled = false;
    var onFailureCalled = false;

    // Act
    var result = await failedResult.MatchAsync(
      onSuccess: async () =>
      {
        onSuccessCalled = true;
        await Task.Delay(1);
        return "Success";
      },
      onFailure: async errors =>
      {
        onFailureCalled = true;
        await Task.Delay(1);
        return $"Failure: {errors.First().Parameters[0]}";
      });

    // Assert
    Assert.False(onSuccessCalled);
    Assert.True(onFailureCalled);
    Assert.Equal("Failure: Test error", result);
  }

  /// <summary>
  /// Validates that the MatchAsync extension method properly propagates exceptions thrown by the async onSuccess function.
  /// This test ensures that exceptions in the async success handling logic are not swallowed but bubble up 
  /// to the calling code for proper error handling.
  /// </summary>
  [Fact]
  public async Task MatchAsync_WhenOnSuccessThrows_PropagatesException()
  {
    // Arrange
    var successResult = Task.FromResult(Result.Ok("test"));

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      await successResult.MatchAsync(
        onSuccess: async value =>
        {
          await Task.Delay(1);
          throw new InvalidOperationException("OnSuccess failed");
        },
        onFailure: async errors =>
        {
          await Task.Delay(1);
          return "Failure";
        }));
  }

  /// <summary>
  /// Validates that the MatchAsync extension method properly propagates exceptions thrown by the async onFailure function.
  /// This test ensures that exceptions in the async failure handling logic are not swallowed but bubble up 
  /// to the calling code for proper error handling.
  /// </summary>
  [Fact]
  public async Task MatchAsync_WhenOnFailureThrows_PropagatesException()
  {
    // Arrange
    var errorMessage = ValidationMessage.CreateError(_errorKey, "Test error");
    var failedResult = Task.FromResult(Result.Create<string>(default, new[] { errorMessage }));

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      await failedResult.MatchAsync(
        onSuccess: async value =>
        {
          await Task.Delay(1);
          return "Success";
        },
        onFailure: async errors =>
        {
          await Task.Delay(1);
          throw new InvalidOperationException("OnFailure failed");
        }));
  }

  #endregion

  #region Integration Tests

  /// <summary>
  /// Validates that the async transformation extension methods (MapAsync, BindAsync, MatchAsync) can be chained together
  /// to create a complete functional pipeline for processing async Results.
  /// This integration test demonstrates how the async transformation methods work together in real-world scenarios
  /// where multiple async operations need to be composed while maintaining proper error handling.
  /// </summary>
  [Fact]
  public async Task AsyncExtensions_CanBeChainedTogether()
  {
    // Arrange
    var initialResult = Task.FromResult(Result.Ok(5));

    // Act
    var finalResult = await initialResult
      .MapAsync(async x =>
      {
        await Task.Delay(1);
        return x * 2;
      })
      .BindAsync(async x =>
      {
        await Task.Delay(1);
        return x > 5 ? Result.Ok(x + 10) : Result.Error(_errorTooSmallKey, "Value too small");
      })
      .MatchAsync(
        onSuccess: async value =>
        {
          await Task.Delay(1);
          return $"Final value: {value}";
        },
        onFailure: async errors =>
        {
          await Task.Delay(1);
          return "Processing failed";
        });

    // Assert
    Assert.Equal("Final value: 20", finalResult);
  }

  /// <summary>
  /// Validates that when a failure occurs in a chained async transformation pipeline, 
  /// subsequent async operations are properly short-circuited and the failure is propagated correctly.
  /// This integration test ensures that the fail-fast behavior works correctly across 
  /// the entire async transformation chain, preventing unnecessary async processing after a failure.
  /// </summary>
  [Fact]
  public async Task AsyncExtensions_WithFailureInChain_StopsProcessing()
  {
    // Arrange
    var initialResult = Result.Ok(2);
    var errorKey = TranslationKeyDefinition.Create("error.too.small", "Value too small")
      .WithStringParameter("message");
    var mapperCalled = false;
    var matchOnSuccessCalled = false;

    // Act
    var finalResult = await initialResult
      .MapAsync(async x => 
      {
        mapperCalled = true;
        await Task.Delay(1);
        return x * 2;
      })
      .BindSync(x => // Use BindSync for synchronous operations
      {
        if (x > 5)
        {
          return Result.Ok(x + 10);
        }
        else
        {
          var errorMessage = ValidationMessage.CreateError(errorKey, "Value too small");
          return Result.Create<int>(default, new[] { errorMessage });
        }
      })
      .MatchAsync(
        onSuccess: async value => 
        {
          matchOnSuccessCalled = true;
          await Task.Delay(1);
          return $"Final value: {value}";
        },
        onFailure: async errors => 
        {
          await Task.Delay(1);
          return $"Processing failed: {errors.First().Parameters[0]}";
        });

    // Assert
    Assert.True(mapperCalled);
    Assert.False(matchOnSuccessCalled);
    Assert.Equal("Processing failed: Value too small", finalResult);
  }

  #endregion
}