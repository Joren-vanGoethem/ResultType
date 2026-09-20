using JV.ResultUtilities;
using JV.ResultUtilities.Exceptions;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests;

/// <summary>
/// Pins the 10.3.0 additions: key metadata, named parameters, materialised message lists,
/// <c>HasError</c> in the package and <c>ResultException.Keys</c>.
/// </summary>
public class KeyMetadataTests
{
    [Fact]
    public void Metadata_IsEmptyByDefault()
    {
        var key = ValidationKeyDefinition.Create("Foo.NotFound");

        Assert.Empty(key.Metadata);
        Assert.False(key.TryGetMetadata<int>("http.status", out _));
    }

    [Fact]
    public void WithMetadata_ReturnsCopyWithEntry_AndLeavesOriginalUntouched()
    {
        var original = ValidationKeyDefinition.Create("Foo.NotFound").WithGuidParameter("id");

        var annotated = original.WithMetadata("http.status", 404);

        Assert.Empty(original.Metadata);
        Assert.Equal(404, annotated.Metadata["http.status"]);
        Assert.Equal(original.Key, annotated.Key);
        Assert.Equal(original.TranslationKey, annotated.TranslationKey);
        Assert.Equal(original.Parameters.Count, annotated.Parameters.Count);
    }

    [Fact]
    public void WithMetadata_ReplacesExistingEntry()
    {
        var key = ValidationKeyDefinition.Create("Foo.Bar")
            .WithMetadata("http.status", 400)
            .WithMetadata("http.status", 409);

        Assert.Equal(409, key.Metadata["http.status"]);
        Assert.Single(key.Metadata);
    }

    [Fact]
    public void Metadata_SurvivesWithFieldName_AndFieldNameSurvivesWithMetadata()
    {
        var a = ValidationKeyDefinition.Create("Foo.Bar").WithMetadata("severity", "warning").WithFieldName("name");
        var b = ValidationKeyDefinition.Create("Foo.Bar").WithFieldName("name").WithMetadata("severity", "warning");

        Assert.Equal("name", a.FieldName);
        Assert.Equal("warning", a.Metadata["severity"]);
        Assert.Equal("name", b.FieldName);
        Assert.Equal("warning", b.Metadata["severity"]);
    }

    [Fact]
    public void TryGetMetadata_IsTyped()
    {
        var key = ValidationKeyDefinition.Create("Foo.Bar").WithMetadata("http.status", 404);

        Assert.True(key.TryGetMetadata<int>("http.status", out var status));
        Assert.Equal(404, status);
        Assert.False(key.TryGetMetadata<string>("http.status", out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void WithMetadata_RejectsBlankName(string name)
    {
        var key = ValidationKeyDefinition.Create("Foo.Bar");

        Assert.Throws<ArgumentException>(() => key.WithMetadata(name, 1));
    }

    [Fact]
    public void WithMetadata_RejectsNullValue()
    {
        var key = ValidationKeyDefinition.Create("Foo.Bar");

        Assert.Throws<ArgumentNullException>(() => key.WithMetadata("x", null!));
    }
}

public class NamedParametersTests
{
    private static readonly ValidationKeyDefinition Range = ValidationKeyDefinition
        .Create("Job.IntervalOutOfRange")
        .WithIntParameter("value")
        .WithIntParameter("minimum")
        .WithIntParameter("maximum");

    [Fact]
    public void Positional_MapsByDeclarationOrder()
    {
        var message = ValidationMessage.Create(Range, [7, 10, 60]);

        Assert.Equal(7, message.NamedParameters["value"]);
        Assert.Equal(10, message.NamedParameters["minimum"]);
        Assert.Equal(60, message.NamedParameters["maximum"]);
        Assert.Equal(["7", "10", "60"], message.Parameters);
    }

    [Fact]
    public void AnonymousObject_MapsByName_RegardlessOfOrder()
    {
        var message = ValidationMessage.Create(Range, new { maximum = 60, value = 7, minimum = 10 });

        Assert.Equal(7, message.NamedParameters["value"]);
        Assert.Equal(60, message.NamedParameters["maximum"]);
        Assert.Equal(["7", "10", "60"], message.Parameters);
    }

    [Fact]
    public void Dictionary_MapsByName()
    {
        var message = ValidationMessage.Create(Range,
            new Dictionary<string, object> { ["value"] = 7, ["minimum"] = 10, ["maximum"] = 60 });

        Assert.Equal(10, message.NamedParameters["minimum"]);
    }

    [Fact]
    public void SingleValue_FillsTheOnlySlot()
    {
        var key = ValidationKeyDefinition.Create("Foo.NotFound").WithGuidParameter("id");
        var id = Guid.NewGuid();

        var message = ValidationMessage.Create(key, id);

        Assert.Equal(id, message.NamedParameters["id"]);
        Assert.Equal(id.ToString(), message.Parameters[0]);
    }

    [Fact]
    public void Defaults_AreAppliedToNamedParameters()
    {
        var key = ValidationKeyDefinition.Create("Foo.TooLong")
            .WithStringParameter("name")
            .WithIntParameter("maxLength", 50);

        var message = ValidationMessage.Create(key, "a-name");

        Assert.Equal("a-name", message.NamedParameters["name"]);
        Assert.Equal(50, message.NamedParameters["maxLength"]);
    }

    [Fact]
    public void NoParameters_GivesEmptyDictionary()
    {
        var message = ValidationMessage.Create(ValidationKeyDefinition.Create("Foo.Required"));

        Assert.Empty(message.NamedParameters);
    }

    [Fact]
    public void Lenient_ZipsByPosition_AndKeysSurplusByIndex()
    {
        var key = ValidationKeyDefinition.Create("Foo.Bar").WithStringParameter("first");

        var message = ValidationMessage.CreateLenient(key, "one", 2);

        Assert.Equal("one", message.NamedParameters["first"]);
        Assert.Equal(2, message.NamedParameters["1"]);
    }

    [Fact]
    public void ResolveParameterValues_AgreesWithFormatParameters()
    {
        object input = new { maximum = 60, value = 7, minimum = 10 };

        var raw = Range.ResolveParameterValues(input);
        var formatted = Range.FormatParameters(input);

        Assert.Equal(formatted, raw.Select(v => v!.ToString()).ToArray());
    }
}

public class MaterialisedMessagesTests
{
    private static readonly ValidationKeyDefinition Key = ValidationKeyDefinition.Create("Foo.Bar");

    [Fact]
    public void LazySequence_IsEnumeratedOnce()
    {
        var enumerations = 0;

        IEnumerable<ValidationMessage> Source()
        {
            enumerations++;
            yield return ValidationMessage.Create(Key);
        }

        var result = Result.Create(Source());

        _ = result.IsFailure;
        _ = result.IsSuccessful;
        _ = result.HasError(Key);
        _ = result.ToString();

        Assert.Equal(1, enumerations);
    }

    [Fact]
    public void CallerList_MutatedAfterwards_DoesNotChangeTheResult()
    {
        var list = new List<ValidationMessage>();
        var result = Result.Create(list);

        list.Add(ValidationMessage.Create(Key));

        Assert.True(result.IsSuccessful);
        Assert.Empty(result.ValidationMessages);
    }

    [Fact]
    public void Merge_ProducesAMaterialisedList()
    {
        var merged = Result.Error(Key).Merge(Result.Error(Key));

        Assert.Equal(2, merged.ValidationMessages.Count);
    }

    [Fact]
    public void ValidationMessages_IsIndexable()
    {
        var result = Result.Error(Key);

        Assert.Equal(Key.Key, result.ValidationMessages[0].KeyDefinition.Key);
    }
}

public class HasErrorTests
{
    private static readonly ValidationKeyDefinition NotFound = ValidationKeyDefinition
        .Create("Backtest.NotFound").WithGuidParameter("id");

    private static readonly ValidationKeyDefinition Required = ValidationKeyDefinition
        .Create("Backtest.StrategyIdRequired");

    private static readonly ValidationKeyDefinition Other = ValidationKeyDefinition
        .Create("Strategy.NotFound").WithGuidParameter("id");

    [Fact]
    public void HasError_TrueForMatchingKey_OnResult()
    {
        var result = Result.Error(NotFound, Guid.NewGuid());

        Assert.True(result.HasError(NotFound));
        Assert.False(result.HasError(Other));
    }

    [Fact]
    public void HasError_TrueForMatchingKey_OnGenericResult()
    {
        var result = Result.Error<int>(NotFound, Guid.NewGuid());

        Assert.True(result.HasError(NotFound));
        Assert.False(result.HasError(Required));
    }

    [Fact]
    public void HasError_FalseOnSuccess()
    {
        Assert.False(Result.Ok().HasError(NotFound));
        Assert.False(Result.Ok(1).HasError(NotFound));
    }

    [Fact]
    public void HasError_MatchesADifferentInstanceWithTheSameKeyString()
    {
        var sameKeyAgain = ValidationKeyDefinition.Create("Backtest.NotFound").WithGuidParameter("id");

        Assert.True(Result.Error(NotFound, Guid.NewGuid()).HasError(sameKeyAgain));
    }

    [Fact]
    public void HasError_ByString()
    {
        Assert.True(Result.Error(Required).HasError("Backtest.StrategyIdRequired"));
        Assert.False(Result.Error(Required).HasError("Backtest.NotFound"));
    }

    [Fact]
    public void HasErrorWithPrefix_MatchesEntityPrefix_Ordinally()
    {
        var result = Result.Error(Required);

        Assert.True(result.HasErrorWithPrefix("Backtest."));
        Assert.False(result.HasErrorWithPrefix("backtest."));
        Assert.False(result.HasErrorWithPrefix("Strategy."));
    }

    [Fact]
    public void MessagesFor_ReturnsOnlyMatchingMessages()
    {
        var result = Result.Create([
            ValidationMessage.Create(Required),
            ValidationMessage.Create(NotFound, Guid.NewGuid()),
            ValidationMessage.Create(NotFound, Guid.NewGuid()),
        ]);

        Assert.Equal(2, result.MessagesFor(NotFound).Count());
        Assert.Empty(result.MessagesFor(Other));
    }

    [Fact]
    public void HasError_RejectsNullKey()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Ok().HasError((ValidationKeyDefinition)null!));
    }
}

public class ResultExceptionKeysTests
{
    [Fact]
    public void Keys_ListsTranslationKeysInOrder()
    {
        var a = ValidationKeyDefinition.Create("A.First");
        var b = ValidationKeyDefinition.Create("B.Second");
        var result = Result.Create([ValidationMessage.Create(a), ValidationMessage.Create(b)]);

        var exception = Assert.Throws<ResultException>(() => result.ThrowIfFailure());

        Assert.Equal(["A.First", "B.Second"], exception.Keys);
        Assert.Equal(2, exception.ValidationMessages.Count);
    }

    [Fact]
    public void ValidationMessages_FromLazySequence_AreMaterialised()
    {
        var enumerations = 0;

        IEnumerable<ValidationMessage> Source()
        {
            enumerations++;
            yield return ValidationMessage.Create(ValidationKeyDefinition.Create("A.First"));
        }

        var exception = new ResultException(Source());

        _ = exception.ValidationMessages.Count;
        _ = exception.Keys;

        Assert.Equal(1, enumerations);
    }
}

public class HttpStatusCodeMetadataTests
{
    private static readonly ValidationKeyDefinition NotFound = ValidationKeyDefinition
        .Create("User.NotFound").WithGuidParameter("id").WithHttpStatusCode(System.Net.HttpStatusCode.NotFound);

    private static readonly ValidationKeyDefinition Conflict = ValidationKeyDefinition
        .Create("User.EmailTaken").WithHttpStatusCode(System.Net.HttpStatusCode.Conflict);

    private static readonly ValidationKeyDefinition Plain = ValidationKeyDefinition
        .Create("User.NameRequired");

    [Fact]
    public void WithHttpStatusCode_StoresUnderTheWellKnownMetadataName()
    {
        Assert.Equal(System.Net.HttpStatusCode.NotFound, NotFound.Metadata[ValidationKeyMetadata.HttpStatusCode]);
    }

    [Fact]
    public void TryGetHttpStatusCode_OnKey_ReadsItBack()
    {
        Assert.True(NotFound.TryGetHttpStatusCode(out var code));
        Assert.Equal(System.Net.HttpStatusCode.NotFound, code);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, NotFound.GetHttpStatusCode());
    }

    [Fact]
    public void TryGetHttpStatusCode_FalseWhenKeyDeclaresNone()
    {
        Assert.False(Plain.TryGetHttpStatusCode(out _));
        Assert.Null(Plain.GetHttpStatusCode());
    }

    [Fact]
    public void TryGetHttpStatusCode_AcceptsAnIntStoredByHand()
    {
        var key = Plain.WithMetadata(ValidationKeyMetadata.HttpStatusCode, 422);

        Assert.True(key.TryGetHttpStatusCode(out var code));
        Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, code);
    }

    [Fact]
    public void TryGetHttpStatusCode_OnMessage_DelegatesToItsKey()
    {
        var message = ValidationMessage.Create(NotFound, Guid.NewGuid());

        Assert.True(message.TryGetHttpStatusCode(out var code));
        Assert.Equal(System.Net.HttpStatusCode.NotFound, code);
    }

    [Fact]
    public void TryGetHttpStatusCode_OnResult_PicksTheHighestDeclaredStatus()
    {
        var result = Result.Create([
            ValidationMessage.Create(Plain),
            ValidationMessage.Create(NotFound, Guid.NewGuid()),
            ValidationMessage.Create(Conflict),
        ]);

        Assert.True(result.TryGetHttpStatusCode(out var code));
        Assert.Equal(System.Net.HttpStatusCode.Conflict, code);
    }

    [Fact]
    public void TryGetHttpStatusCode_OnResult_FalseOnSuccessOrWhenNoKeyDeclaresOne()
    {
        Assert.False(Result.Ok().TryGetHttpStatusCode(out _));
        Assert.False(Result.Error(Plain).TryGetHttpStatusCode(out _));
        Assert.Null(Result.Error<int>(Plain).GetHttpStatusCode());
    }

    [Fact]
    public void TryGetHttpStatusCode_OnGenericResult_Works()
    {
        var result = Result.Error<int>(NotFound, Guid.NewGuid());

        Assert.Equal(System.Net.HttpStatusCode.NotFound, result.GetHttpStatusCode());
    }
}
