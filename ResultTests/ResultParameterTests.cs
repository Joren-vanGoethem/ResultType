using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests;

public class ResultParameterTests
{
    [Fact]
    public void ResultError_WithAnonymousObject_CreatesValidResult()
    {
        // Arrange
        var key = ValidationKeyDefinition.Create("test.key", "Hello {name}")
            .WithStringParameter("name");
        
        // Act
        var result = Result.Error(key, new { name = "John" });
        
        // Assert
        Assert.False(result.IsSuccessful);
        var message = result.ValidationMessages.First();
        Assert.Equal("John", message.Parameters[0]);
        Assert.Contains("John", message.MapToErrorMessage());
    }

    [Fact]
    public void ResultError_WithDefaultValues_CreatesValidResult()
    {
        // Arrange
        var key = ValidationKeyDefinition.Create("test.key", "Age of {name} is {age}")
            .WithStringParameter("name")
            .WithIntParameter("age", 30);
        
        // Act - Only passing name, age should use default
        var result = Result.Error(key, new { name = "John" });
        
        // Assert
        Assert.False(result.IsSuccessful);
        var message = result.ValidationMessages.First();
        Assert.Equal("John", message.Parameters[0]);
        Assert.Equal("30", message.Parameters[1]);
    }

    [Fact]
    public void ResultError_WithDictionary_CreatesValidResult()
    {
        // Arrange
        var key = ValidationKeyDefinition.Create("test.key", "{name}: {score}")
            .WithStringParameter("name")
            .WithIntParameter("score");
        
        var parameters = new Dictionary<string, object>
        {
            { "name", "Alice" },
            { "score", 100 }
        };
        
        // Act
        var result = Result.Error(key, parameters);
        
        // Assert
        Assert.False(result.IsSuccessful);
        var message = result.ValidationMessages.First();
        Assert.Equal("Alice", message.Parameters[0]);
        Assert.Equal("100", message.Parameters[1]);
    }

    [Fact]
    public void ResultError_WithSingleValue_WhenOneRequired_CreatesValidResult()
    {
        // Arrange
        var key = ValidationKeyDefinition.Create("test.key", "Value: {val}")
            .WithIntParameter("val");
        
        // Act
        var result = Result.Error(key, 42);
        
        // Assert
        Assert.False(result.IsSuccessful);
        var message = result.ValidationMessages.First();
        Assert.Equal("42", message.Parameters[0]);
    }

    [Fact]
    public void ResultError_WithNull_WhenAllHaveDefaults_CreatesValidResult()
    {
        // Arrange
        var key = ValidationKeyDefinition.Create("test.key", "Default")
            .WithStringParameter("p1", "d1")
            .WithIntParameter("p2", 2);
        
        // Act
        var result = Result.Error(key, null!);
        
        // Assert
        Assert.False(result.IsSuccessful);
        var message = result.ValidationMessages.First();
        Assert.Equal("d1", message.Parameters[0]);
        Assert.Equal("2", message.Parameters[1]);
    }
}
