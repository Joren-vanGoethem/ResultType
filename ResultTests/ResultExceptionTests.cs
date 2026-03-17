using JV.ResultUtilities;
using JV.ResultUtilities.Exceptions;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests;

public class ResultExceptionTests
{
    private static readonly ValidationKeyDefinition TestKey = ValidationKeyDefinition
        .Create("test.error")
        .WithStringParameter("message");

    [Fact]
    public void ResultException_FromResult_SetsMessage()
    {
        var result = Result.Error(TestKey, "something failed");
        var ex = new ResultException(result);

        Assert.Contains("test.error", ex.Message);
        Assert.Contains("something failed", ex.Message);
        Assert.Single(ex.ValidationMessages);
    }

    [Fact]
    public void ResultException_FromValidationMessage_SetsMessage()
    {
        var msg = ValidationMessage.Create(TestKey, "bad input");
        var ex = new ResultException(msg);

        Assert.Contains("test.error", ex.Message);
        Assert.Contains("bad input", ex.Message);
    }

    [Fact]
    public void ResultException_FromValidationKey_SetsMessage()
    {
        var key = ValidationKeyDefinition.Create("simple.key");
        var ex = new ResultException(key);

        Assert.Contains("simple.key", ex.Message);
        Assert.Single(ex.ValidationMessages);
    }
    
    [Fact]
    public void ResultException_MultipleMessages_JoinsWithSemicolon()
    {
        var msg1 = ValidationMessage.Create(TestKey, "error one");
        var msg2 = ValidationMessage.Create(TestKey, "error two");
        var result = Result.Create(new[] { msg1, msg2 });
        var ex = new ResultException(result);

        Assert.Contains("; ", ex.Message);
        Assert.Contains("error one", ex.Message);
        Assert.Contains("error two", ex.Message);
    }
}
