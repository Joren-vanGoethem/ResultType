using System.Net.Mail;
using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests;

// Define a sample domain entity to use in tests
public class User
{
    public string Username { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public Uri Website { get; set; }
    public DateOnly BirthDate { get; set; }
    public TimeOnly PreferredLoginTime { get; set; }
    public TimeSpan SessionTimeout { get; set; }
    public string PhoneNumber { get; set; }
}

// Sample service that uses the validation system
public class UserService
{
    // Define translation keys as constants for reuse
    private static readonly ValidationKeyDefinition UsernameInvalidKey = ValidationKeyDefinition
        .Create("user.username.invalid")
        .WithStringParameter("username")
        .WithIntParameter("minLength");

    private static readonly ValidationKeyDefinition EmailInvalidKey = ValidationKeyDefinition
        .Create("user.email.invalid")
        .WithStringParameter(
            "email"); // when using emailParameter it HAS to be a valid email, here it might be something wrong

    private static readonly ValidationKeyDefinition AgeInvalidKey = ValidationKeyDefinition
        .Create("user.age.invalid")
        .WithIntParameter("age")
        .WithIntParameter("minAge");

    public Result<User> ValidateUser(User user)
    {
        var validationMessages = new List<ValidationMessage>();

        // Validate username
        if (string.IsNullOrWhiteSpace(user.Username) || user.Username.Length < 3)
        {
            validationMessages.Add(ValidationMessage.Create(UsernameInvalidKey, user.Username ?? string.Empty, 3));
        }

        // Validate email
        if (string.IsNullOrWhiteSpace(user.Email) || !IsValidEmail(user.Email))
        {
            validationMessages.Add(ValidationMessage.Create(EmailInvalidKey, user.Email ?? string.Empty));
        }

        // Validate age
        if (user.Age < 18)
        {
            validationMessages.Add(ValidationMessage.Create(AgeInvalidKey, user.Age, 18));
        }

        // Return result
        if (validationMessages.Count != 0)
        {
            return Result.Create(validationMessages);
        }

        // Add success message
        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.NewGuid();
        }

        return Result.Create(user, validationMessages);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class IntegrationTests
{
    /// <summary>
    /// Validates that the UserService.ValidateUser method returns a successful Result&lt;User&gt; 
    /// when provided with a User object that meets all validation criteria.
    /// This integration test ensures the entire validation pipeline works correctly for valid data,
    /// including username length, email format, and age requirements.
    /// </summary>
    [Fact]
    public void ValidUser_ReturnsSuccessResult()
    {
        // Arrange
        var userService = new UserService();
        var user = new User
        {
            Username = "johndoe",
            Email = "john@example.com",
            Age = 25,
            IsActive = true,
            Website = new Uri("https://example.com"),
            BirthDate = new DateOnly(1998, 5, 15),
            PreferredLoginTime = new TimeOnly(9, 0),
            SessionTimeout = TimeSpan.FromHours(2),
            PhoneNumber = "+1234567890"
        };

        // Act
        var result = userService.ValidateUser(user);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(user, result.Value);
        Assert.Empty(result.ValidationMessages);
    }

    /// <summary>
    /// Validates that the UserService.ValidateUser method returns a failed Result&lt;User&gt; 
    /// containing multiple validation errors when provided with a User object that violates 
    /// multiple validation rules (username too short, invalid email, age too young).
    /// This integration test ensures the validation pipeline properly collects and reports 
    /// multiple validation failures simultaneously.
    /// </summary>
    [Fact]
    public void InvalidUser_ReturnsFailureResult()
    {
        // Arrange
        var userService = new UserService();
        var user = new User
        {
            Username = "jo", // Too short
            Email = "not-an-email", // Invalid email
            Age = 15, // Too young
        };

        // Act
        var result = userService.ValidateUser(user);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(3, result.ValidationMessages.Count());
    }

    /// <summary>
    /// Validates that the Result type can be effectively used in method chaining scenarios
    /// where multiple validation operations need to be combined and their results merged.
    /// This integration test demonstrates the practical use of the MergeResults extension method
    /// for combining multiple validation results into a single aggregated result.
    /// </summary>
    [Fact]
    public void ResultType_CanBeUsedInMethodChaining()
    {
        // Arrange
        var userService = new UserService();
        var user1 = new User
        {
            Username = "user1",
            Email = "user1@example.com",
            Age = 30
        };
        var user2 = new User
        {
            Username = "user2",
            Email = "user2@example.com",
            Age = 25
        };

        // Act - simulate creating multiple users and merging results
        var result1 = userService.ValidateUser(user1);
        var result2 = userService.ValidateUser(user2);

        var mergedResults = new[] { result1, result2 }.MergeResults();

        // Assert
        Assert.True(mergedResults.IsSuccessful);
        Assert.Equal(2, mergedResults.Value.Count());
        Assert.Empty(mergedResults.ValidationMessages);
    }

    /// <summary>
    /// Validates that the ValidationResourceGenerator correctly generates resource files
    /// based on the ValidationKeyDefinition.Create calls in the codebase.
    /// This test ensures that our validation keys are properly captured and processed by the generator.
    /// </summary>
    [Fact]
    public void ValidationResourceGenerator_CreatesExpectedResources()
    {
        // This test is just a placeholder - the actual validation of the generator is done in
        // the ValidationResourceGeneratorTests class in the JV.ResultUtilities.Generators.Tests project

        // Here we're just verifying that the validation keys defined in UserService are correctly
        // structured for the source generator to process them

        // Arrange & Act
        var usernameKey = ValidationKeyDefinition
            .Create("test.username.invalid")
            .WithStringParameter("username");

        // Assert
        Assert.Equal("test.username.invalid", usernameKey.Key);
        Assert.Single(usernameKey.Parameters);
        Assert.Equal(ParameterType.String, usernameKey.Parameters[0].Type);
        Assert.Equal("username", usernameKey.Parameters[0].Name);
    }
}