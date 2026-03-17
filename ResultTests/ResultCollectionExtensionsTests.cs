using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests
{
    public class ResultCollectionExtensionsTests
    {
        private static readonly ValidationKeyDefinition ProcessingErrorKey = ValidationKeyDefinition
            .Create("processing.error")
            .WithStringParameter("item");

        private static readonly ValidationKeyDefinition ValidationErrorKey = ValidationKeyDefinition
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

        private static Result<string> ValidateUser(dynamic request)
        {
            var errors = new List<ValidationMessage>();

            if (request.Age < 18)
                errors.Add(ValidationMessage.Create(ValidationErrorKey, request.Age));

            if (!request.Email.Contains("@") || request.Email.Contains("invalid"))
                errors.Add(ValidationMessage.Create(ProcessingErrorKey, request.Email));

            return errors.Any()
                ? Result.Create<string>(null, errors)
                : Result.Ok($"ValidUser-{request.Name}");
        }
    }
}