using JV.Utils;
using JV.Utils.Extensions;

namespace ResultTests;

public class AsyncResultExtensionsTests
{
  private readonly TranslationKeyDefinition _errorKey = TranslationKeyDefinition
    .Create("error.test")
    .WithStringParameter("message");
  
  private readonly TranslationKeyDefinition _errorNegativeKey = TranslationKeyDefinition
    .Create("error.negative")
    .WithStringParameter("message");
  
 private readonly TranslationKeyDefinition _errorTooSmallKey = TranslationKeyDefinition
    .Create("error.too.small")
    .WithStringParameter("message");

  #region MapAsync Tests

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

  #endregion

  #region BindAsync Tests

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

  #endregion

  #region MatchAsync Tests - Task<Result<T>> with async handlers

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

  #endregion

  #region MatchAsync Tests - Task<Result<T>> with sync handlers

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

  #endregion

  #region MatchAsync Tests - Result<T> with async handlers

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

  #endregion

  #region MatchAsync Tests - Task<Result> (non-generic)

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

  #endregion

  #region Error Handling Tests

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