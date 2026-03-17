using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;
using JV.ResultUtilities.ValidationPipeline;

namespace ResultTests;

public class ValidationPipelineTests
{
    private static readonly ValidationKeyDefinition TooShortKey = ValidationKeyDefinition
        .Create("validation.too.short")
        .WithIntParameter("length");

    private static readonly ValidationKeyDefinition TooLongKey = ValidationKeyDefinition
        .Create("validation.too.long")
        .WithIntParameter("length");

    private static readonly ValidationKeyDefinition AsyncErrorKey = ValidationKeyDefinition
        .Create("validation.async.error")
        .WithStringParameter("detail");

    [Fact]
    public void Validate_WithSyncRules_ReturnsSuccess()
    {
        var pipeline = new ValidationPipeline<string>()
            .AddRule(s => s.Length >= 3 ? Result.Ok() : Result.Error(TooShortKey, s.Length));

        var result = pipeline.Validate("hello");

        Assert.True(result.IsSuccessful);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Validate_WithSyncRules_ReturnsFailure()
    {
        var pipeline = new ValidationPipeline<string>()
            .AddRule(s => s.Length >= 3 ? Result.Ok() : Result.Error(TooShortKey, s.Length));

        var result = pipeline.Validate("ab");

        Assert.True(result.IsFailure);
        Assert.Single(result.ValidationMessages);
    }

    [Fact]
    public void Validate_WithAsyncRules_ThrowsInvalidOperationException()
    {
        var pipeline = new ValidationPipeline<string>()
            .AddRule(s => s.Length >= 3 ? Result.Ok() : Result.Error(TooShortKey, s.Length))
            .AddRule(async s =>
            {
                await Task.Delay(1);
                return Result.Ok();
            });

        Assert.Throws<InvalidOperationException>(() => pipeline.Validate("hello"));
    }

    [Fact]
    public async Task ValidateAsync_WithAsyncRules_ReturnsSuccess()
    {
        var pipeline = new ValidationPipeline<string>()
            .AddRule(s => s.Length >= 3 ? Result.Ok() : Result.Error(TooShortKey, s.Length))
            .AddRule(async s =>
            {
                await Task.Delay(1);
                return s.Length <= 100 ? Result.Ok() : Result.Error(TooLongKey, s.Length);
            });

        var result = await pipeline.ValidateAsync("hello");

        Assert.True(result.IsSuccessful);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public async Task ValidateAsync_CollectsAllErrors()
    {
        var pipeline = new ValidationPipeline<string>()
            .AddRule(s => s.Length >= 5 ? Result.Ok() : Result.Error(TooShortKey, s.Length))
            .AddRule(s => s.Length <= 2 ? Result.Ok() : Result.Error(TooLongKey, s.Length));

        var result = await pipeline.ValidateAsync("abc");

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.ValidationMessages.Count());
    }

    [Fact]
    public void ShortCircuit_StopsOnFirstFailure()
    {
        var secondRuleCalled = false;
        var pipeline = new ValidationPipeline<string> { ShortCircuit = true }
            .AddRule(s => s.Length >= 5 ? Result.Ok() : Result.Error(TooShortKey, s.Length))
            .AddRule(s =>
            {
                secondRuleCalled = true;
                return s.Length <= 2 ? Result.Ok() : Result.Error(TooLongKey, s.Length);
            });

        var result = pipeline.Validate("abc");

        Assert.True(result.IsFailure);
        Assert.Single(result.ValidationMessages);
        Assert.False(secondRuleCalled);
    }

    [Fact]
    public async Task ShortCircuit_Async_StopsOnFirstFailure()
    {
        var asyncRuleCalled = false;
        var pipeline = new ValidationPipeline<string> { ShortCircuit = true }
            .AddRule(s => s.Length >= 5 ? Result.Ok() : Result.Error(TooShortKey, s.Length))
            .AddRule(async s =>
            {
                asyncRuleCalled = true;
                await Task.Delay(1);
                return Result.Ok();
            });

        var result = await pipeline.ValidateAsync("abc");

        Assert.True(result.IsFailure);
        Assert.Single(result.ValidationMessages);
        Assert.False(asyncRuleCalled);
    }

    [Fact]
    public void ShortCircuit_AllPass_ReturnsSuccess()
    {
        var pipeline = new ValidationPipeline<string> { ShortCircuit = true }
            .AddRule(s => s.Length >= 3 ? Result.Ok() : Result.Error(TooShortKey, s.Length))
            .AddRule(s => s.Length <= 100 ? Result.Ok() : Result.Error(TooLongKey, s.Length));

        var result = pipeline.Validate("hello");

        Assert.True(result.IsSuccessful);
    }
}
