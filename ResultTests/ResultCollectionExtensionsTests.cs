using JV.Utils;
using JV.Utils.Extensions;
using JV.Utils.ValidationMessage;

namespace ResultTests
{
    public class ResultCollectionExtensionsTests
    {
        private static readonly TranslationKeyDefinition ProcessingErrorKey = TranslationKeyDefinition
            .Create("processing.error")
            .WithStringParameter("item");

        private static readonly TranslationKeyDefinition ValidationErrorKey = TranslationKeyDefinition
            .Create("validation.error")
            .WithIntParameter("value");

        [Fact]
        public void TraverseAll_WithAllSuccessfulTransforms_ReturnsAllResults()
        {
            // Arrange
            var source = new[] { 1, 2, 3, 4, 5 };

            // Act
            var result = source.TraverseAll(x => Result.Ok(x * 2));

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(new[] { 2, 4, 6, 8, 10 }, result.Value);
        }

        [Fact]
        public void TraverseAll_WithSomeFailures_ReturnsFailureWithAllErrors()
        {
            // Arrange
            var source = new[] { 1, 2, 3, 4, 5 };

            // Act
            var result = source.TraverseAll(x =>
                x % 2 == 0
                    ? Result.Error(ValidationErrorKey, x)
                    : Result.Ok(x * 2));

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(2, result.ValidationMessages.Count()); // For items 2 and 4
        }

        [Fact]
        public void TraverseAll_WithEmptyCollection_ReturnsEmptySuccess()
        {
            // Arrange
            var source = Array.Empty<int>();

            // Act
            var result = source.TraverseAll(x => Result.Ok(x.ToString()));

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task TraverseAllAsync_WithAllSuccessfulTransforms_ReturnsAllResults()
        {
            // Arrange
            var source = new[] { "a", "b", "c" };

            // Act
            var result = await source.TraverseAllAsync(async x =>
            {
                await Task.Delay(1);
                return Result.Ok(x.ToUpper());
            });

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(new[] { "A", "B", "C" }, result.Value);
        }

        [Fact]
        public async Task TraverseAllAsync_WithSomeFailures_ReturnsFailureWithAllErrors()
        {
            // Arrange
            var source = new[] { "valid", "invalid", "ok", "bad" };

            // Act
            var result = await source.TraverseAllAsync(async x =>
            {
                await Task.Delay(1);
                return x.Contains("invalid") || x.Contains("bad")
                    ? Result.Error(ProcessingErrorKey, x)
                    : Result.Ok(x.ToUpper());
            });

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(2, result.ValidationMessages.Count());
        }

        [Fact]
        public async Task TraverseAllAsync_WithEmptyCollection_ReturnsEmptySuccess()
        {
            // Arrange
            var source = Array.Empty<string>();

            // Act
            var result = await source.TraverseAllAsync(async x =>
            {
                await Task.Delay(1);
                return Result.Ok(x.Length);
            });

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Value);
        }

        [Fact]
        public void TraversePartial_WithMixedResults_ReturnsOnlySuccessful()
        {
            // Arrange
            var source = new[] { 1, 2, 3, 4, 5, 6 };

            // Act
            var result = source.TraversePartial(x =>
                x % 3 == 0
                    ? Result.Error(ValidationErrorKey, x)
                    : Result.Ok($"Item-{x}"));

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(4, result.Value.Count()); // All except 3 and 6
            Assert.Contains("Item-1", result.Value);
            Assert.Contains("Item-2", result.Value);
            Assert.Contains("Item-4", result.Value);
            Assert.Contains("Item-5", result.Value);
        }

        [Fact]
        public void TraversePartial_WithAllFailures_ReturnsEmptySuccess()
        {
            // Arrange
            var source = new[] { 1, 2, 3 };

            // Act
            var result = source.TraversePartial<int, string>(x => Result.Error(ValidationErrorKey, x));

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task TraversePartialAsync_WithMixedResults_ReturnsOnlySuccessful()
        {
            // Arrange
            var source = new[] { "apple", "banana", "cherry", "date" };

            // Act
            var result = await source.TraversePartialAsync(async x =>
            {
                await Task.Delay(1);
                return x.Length > 5
                    ? Result.Error(ProcessingErrorKey, x)
                    : Result.Ok(x.Length);
            });

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(new[] { 5, 4 }, result.Value); // "apple" and "date"
        }

        [Fact]
        public async Task TraversePartialAsync_WithAllFailures_ReturnsEmptySuccess()
        {
            // Arrange
            var source = new[] { "verylongstring", "anotherlongstring" };

            // Act
            var result = await source.TraversePartialAsync(async x =>
            {
                await Task.Delay(1);
                return x.Length > 5
                    ? Result.Error(ProcessingErrorKey, x)
                    : Result.Ok(x.Length);
            });

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Value);
        }

        [Fact]
        public void TraverseAll_RealWorldScenario_UserValidation()
        {
            // Arrange
            var userRequests = new[]
            {
                new { Name = "John", Age = 25, Email = "john@test.com" },
                new { Name = "Jane", Age = 17, Email = "jane@test.com" }, // Under age
                new { Name = "Bob", Age = 30, Email = "invalid-email" }, // Invalid email
                new { Name = "Alice", Age = 28, Email = "alice@test.com" }
            };

            // Act
            var result = userRequests.TraverseAll(ValidateUser);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(2, result.ValidationMessages.Count()); // Jane and Bob should fail
        }

        [Fact]
        public void TraversePartial_RealWorldScenario_UserValidation()
        {
            // Arrange
            var userRequests = new[]
            {
                new { Name = "John", Age = 25, Email = "john@test.com" },
                new { Name = "Jane", Age = 17, Email = "jane@test.com" }, // Under age
                new { Name = "Bob", Age = 30, Email = "invalid-email" }, // Invalid email
                new { Name = "Alice", Age = 28, Email = "alice@test.com" }
            };

            // Act
            var result = userRequests.TraversePartial(ValidateUser);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(2, result.Value.Count()); // Only John and Alice should succeed
        }

        private static Result<string> ValidateUser(dynamic request)
        {
            var errors = new List<ValidationMessage>();

            if (request.Age < 18)
                errors.Add(ValidationMessage.CreateError(ValidationErrorKey, request.Age));

            if (!request.Email.Contains("@") || request.Email.Contains("invalid"))
                errors.Add(ValidationMessage.CreateError(ProcessingErrorKey, request.Email));

            return errors.Any()
                ? Result.Create<string>(null, errors)
                : Result.Ok($"ValidUser-{request.Name}");
        }
    }
}