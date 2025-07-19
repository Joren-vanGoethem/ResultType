using JV.Utils;
using JV.Utils.Extensions;
using JV.Utils.ValidationMessage;

namespace ResultTests
{
    public class ResultExtensionsTests
    {
        private static readonly ValidationKeyDefinition TestErrorKey = ValidationKeyDefinition
            .Create("test.error")
            .WithStringParameter("message");

        private static readonly ValidationKeyDefinition ValidationErrorKey = ValidationKeyDefinition
            .Create("validation.error")
            .WithIntParameter("value");

        [Fact]
        public void MergeResults_WithAllSuccessfulResults_ReturnsSuccessfulResult()
        {
            // Arrange
            var results = new[]
            {
                Result.Ok(),
                Result.Ok(),
                Result.Ok()
            };

            // Act
            var mergedResult = results.MergeResults();

            // Assert
            Assert.True(mergedResult.IsSuccessful);
            Assert.Empty(mergedResult.ValidationMessages);
        }

        [Fact]
        public void MergeResults_WithSomeFailures_ReturnsFailureWithAllErrors()
        {
            // Arrange
            var results = new[]
            {
                Result.Ok(),
                Result.Error(TestErrorKey, "First error"),
                Result.Error(TestErrorKey, "Second error")
            };

            // Act
            var mergedResult = results.MergeResults();

            // Assert
            Assert.True(mergedResult.IsFailure);
            Assert.Equal(2, mergedResult.ValidationMessages.Count());
        }

        [Fact]
        public void MergeResults_Generic_WithAllSuccessfulResults_ReturnsAllValues()
        {
            // Arrange
            var results = new[]
            {
                Result.Ok(1),
                Result.Ok(2),
                Result.Ok(3)
            };

            // Act
            var mergedResult = results.MergeResults();

            // Assert
            Assert.True(mergedResult.IsSuccessful);
            Assert.Equal(new[] { 1, 2, 3 }, mergedResult.Value);
        }

        [Fact]
        public void MergeResults_Generic_WithFailures_ReturnsFailureWithErrors()
        {
            // Arrange
            var results = new[]
            {
                Result.Ok(1),
                Result.Error(ValidationErrorKey, 2),
                Result.Ok(3)
            };

            // Act
            var mergedResult = results.MergeResults();

            // Assert
            Assert.True(mergedResult.IsFailure);
            Assert.Single(mergedResult.ValidationMessages);
        }

        [Fact]
        public async Task MergeResults_AsyncTasks_WithAllSuccessful_ReturnsSuccessful()
        {
            // Arrange
            var resultTasks = new[]
            {
                Task.FromResult(Result.Ok()),
                Task.FromResult(Result.Ok()),
                Task.FromResult(Result.Ok())
            };

            // Act
            var mergedResult = await resultTasks.MergeResults();

            // Assert
            Assert.True(mergedResult.IsSuccessful);
        }

        [Fact]
        public async Task MergeResults_AsyncTasks_WithFailures_ReturnsFailure()
        {
            // Arrange
            var resultTasks = new[]
            {
                Task.FromResult(Result.Ok()),
                Task.FromResult(Result.Error(TestErrorKey, "Error")),
                Task.FromResult(Result.Ok())
            };

            // Act
            var mergedResult = await resultTasks.MergeResults();

            // Assert
            Assert.True(mergedResult.IsFailure);
            Assert.Single(mergedResult.ValidationMessages);
        }

        [Fact]
        public async Task MergeResults_AsyncGeneric_WithAllSuccessful_ReturnsAllValues()
        {
            // Arrange
            var resultTasks = new[]
            {
                Task.FromResult(Result.Ok("A")),
                Task.FromResult(Result.Ok("B")),
                Task.FromResult(Result.Ok("C"))
            };

            // Act
            var mergedResult = await resultTasks.MergeResults();

            // Assert
            Assert.True(mergedResult.IsSuccessful);
            Assert.Equal(new[] { "A", "B", "C" }, mergedResult.Value);
        }

        [Fact]
        public void OnSuccess_WithSuccessfulResult_ExecutesNextFunction()
        {
            // Arrange
            var result = Result.Ok();
            var executed = false;

            // Act
            var finalResult = result.OnSuccess(() =>
            {
                executed = true;
                return Result.Ok();
            });

            // Assert
            Assert.True(executed);
            Assert.True(finalResult.IsSuccessful);
        }

        [Fact]
        public void OnSuccess_WithFailedResult_DoesNotExecuteNextFunction()
        {
            // Arrange
            var result = Result.Error(TestErrorKey, "Error");
            var executed = false;

            // Act
            var finalResult = result.OnSuccess(() =>
            {
                executed = true;
                return Result.Ok();
            });

            // Assert
            Assert.False(executed);
            Assert.True(finalResult.IsFailure);
        }

        [Fact]
        public void OnSuccess_Collection_WithAllSuccessful_ReturnsSuccessful()
        {
            // Arrange
            var results = new[]
            {
                Result.Ok(),
                Result.Ok(),
                Result.Ok()
            };

            // Act
            var finalResult = results.OnSuccess();

            // Assert
            Assert.True(finalResult.IsSuccessful);
        }

        [Fact]
        public void OnSuccess_Collection_WithFailure_StopsOnFirstFailure()
        {
            // Arrange
            var callCount = 0;

            // Use lazy evaluation with functions instead of pre-computed results
            var resultFunctions = new List<Func<Result>>
            {
                () => CreateCountingResult(ref callCount, true), // Should be called (success)
                () => CreateCountingResult(ref callCount, false), // Should be called (failure)
                () => CreateCountingResult(ref callCount, true) // Should NOT be called (short-circuit)
            };

            // Act - Convert functions to results using OnSuccess chaining
            var finalResult = resultFunctions.Aggregate(Result.Ok(), (prev, func) =>
                prev.OnSuccess(func));

            // Assert
            Assert.True(finalResult.IsFailure);
            Assert.Equal(2, callCount); // Only first two should be evaluated
        }


        [Fact]
        public void Ensure_WithPredicateTrue_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result.Ok(10);

            // Act
            var ensuredResult = result.Ensure(x => x > 5, ValidationErrorKey, 10);

            // Assert
            Assert.True(ensuredResult.IsSuccessful);
            Assert.Equal(10, ensuredResult.Value);
        }

        [Fact]
        public void Ensure_WithPredicateFalse_ReturnsFailure()
        {
            // Arrange
            var result = Result.Ok(3);

            // Act
            var ensuredResult = result.Ensure(x => x > 5, ValidationErrorKey, 3);

            // Assert
            Assert.True(ensuredResult.IsFailure);
            Assert.Single(ensuredResult.ValidationMessages);
        }

        [Fact]
        public void Ensure_WithFailedResult_ReturnsOriginalFailure()
        {
            // Arrange
            Result<int> result = Result.Error(TestErrorKey, "Original error");

            // Act
            var ensuredResult = result.Ensure(x => x > 5, ValidationErrorKey, 0);

            // Assert
            Assert.True(ensuredResult.IsFailure);
            Assert.Single(ensuredResult.ValidationMessages);
        }

        [Fact]
        public void Filter_WithPredicateTrue_ReturnsOriginalResult()
        {
            // Arrange
            var result = Result.Ok("valid");

            // Act
            var filteredResult = result.Filter(x => x.Length > 3, TestErrorKey, "too short");

            // Assert
            Assert.True(filteredResult.IsSuccessful);
            Assert.Equal("valid", filteredResult.Value);
        }

        [Fact]
        public void Filter_WithPredicateFalse_ReturnsFailure()
        {
            // Arrange
            var result = Result.Ok("no");

            // Act
            var filteredResult = result.Filter(x => x.Length > 3, TestErrorKey, "too short");

            // Assert
            Assert.True(filteredResult.IsFailure);
            Assert.Single(filteredResult.ValidationMessages);
        }

        [Fact]
        public void Do_WithSuccessfulResult_ExecutesAction()
        {
            // Arrange
            var result = Result.Ok(42);
            var capturedValue = 0;

            // Act
            var finalResult = result.Do(value => capturedValue = value);

            // Assert
            Assert.Equal(42, capturedValue);
            Assert.Equal(result, finalResult); // Should return same result
        }

        [Fact]
        public void Do_WithFailedResult_DoesNotExecuteAction()
        {
            // Arrange
            Result<int> result = Result.Error(TestErrorKey, "Error");
            var actionExecuted = false;

            // Act
            var finalResult = result.Do(value => actionExecuted = true);

            // Assert
            Assert.False(actionExecuted);
            Assert.Equal(result, finalResult); // Should return same result
        }

        [Fact]
        public async Task DoAsync_WithSuccessfulResult_ExecutesAsyncAction()
        {
            // Arrange
            var result = Result.Ok("test");
            var capturedValue = string.Empty;

            // Act
            var finalResult = await result.DoAsync(async value =>
            {
                await Task.Delay(1); // Simulate async work
                capturedValue = value;
            });

            // Assert
            Assert.Equal("test", capturedValue);
            Assert.Equal(result.Value, finalResult.Value);
            Assert.True(finalResult.IsSuccessful);
        }

        [Fact]
        public async Task DoAsync_WithFailedResult_DoesNotExecuteAction()
        {
            // Arrange
            Result<string> result = Result.Error(TestErrorKey, "Error");
            var actionExecuted = false;

            // Act
            var finalResult = await result.DoAsync(async value =>
            {
                await Task.Delay(1);
                actionExecuted = true;
            });

            // Assert
            Assert.False(actionExecuted);
            Assert.True(finalResult.IsFailure);
        }

        private static Result CreateCountingResult(ref int counter, bool shouldSucceed)
        {
            counter++;
            return shouldSucceed ? Result.Ok() : Result.Error(TestErrorKey, "Failed");
        }
    }
}