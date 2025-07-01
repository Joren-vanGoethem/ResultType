using System;
using System.Collections.Generic;
using System.Linq;
using JV.Utils;
using JV.Utils.Extensions;
using Xunit;

namespace TestProject1;

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
    private static readonly TranslationKeyDefinition UsernameInvalidKey = TranslationKeyDefinition
        .Create("user.username.invalid")
        .WithStringParameter("username")
        .WithIntParameter("minLength");

    private static readonly TranslationKeyDefinition EmailInvalidKey = TranslationKeyDefinition
        .Create("user.email.invalid")
        .WithStringParameter("email"); // when using emailParameter it HAS to be a valid email, here it might be something wrong

    private static readonly TranslationKeyDefinition AgeInvalidKey = TranslationKeyDefinition
        .Create("user.age.invalid")
        .WithIntParameter("age")
        .WithIntParameter("minAge");

    public Result<User> ValidateUser(User user)
    {
        var validationMessages = new List<ValidationMessage>();

        // Validate username
        if (string.IsNullOrWhiteSpace(user.Username) || user.Username.Length < 3)
        {
            validationMessages.Add(ValidationMessage.CreateError(UsernameInvalidKey, user.Username ?? string.Empty, 3));
        }

        // Validate email
        if (string.IsNullOrWhiteSpace(user.Email) || !IsValidEmail(user.Email))
        {
            validationMessages.Add(ValidationMessage.CreateError(EmailInvalidKey, user.Email ?? string.Empty));
        }

        // Validate age
        if (user.Age < 18)
        {
            validationMessages.Add(ValidationMessage.CreateError(AgeInvalidKey, user.Age, 18));
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
            var addr = new System.Net.Mail.MailAddress(email);
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
        Assert.Equal(0, mergedResults.ValidationMessages.Count());
    }
}
