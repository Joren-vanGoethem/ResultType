using JV.ResultUtilities;
using JV.ResultUtilities.AspNetCore;
using JV.ResultUtilities.Exceptions;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCoreTests;

/// <summary>Captures the ProblemDetails instead of writing it to a response body.</summary>
internal sealed class CapturingProblemDetailsService : IProblemDetailsService
{
    public ProblemDetails? Captured { get; private set; }

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        Captured = context.ProblemDetails;
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
    {
        Captured = context.ProblemDetails;
        return ValueTask.FromResult(true);
    }
}

/// <summary>Records every log call so the level and structured state can be asserted.</summary>
internal sealed class RecordingLogger : ILogger<ResultExceptionHandler>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
        => Entries.Add((logLevel, formatter(state, exception)));
}

internal sealed class UpperCaseTranslator : IResultMessageTranslator
{
    public string Translate(ValidationMessage message)
        => message.TranslationKey.ToUpperInvariant() + (message.Parameters.Length > 0 ? ":" + string.Join(",", message.Parameters) : "");
}

internal sealed class EmptyTranslator : IResultMessageTranslator
{
    public string Translate(ValidationMessage message) => "";
}

public class ResultExceptionHandlerTests
{
    private static readonly ValidationKeyDefinition NotFound = ValidationKeyDefinition
        .Create("User.NotFound").WithGuidParameter("id").WithHttpStatus(404);

    private static readonly ValidationKeyDefinition NameRequired = ValidationKeyDefinition
        .Create("User.NameRequired").WithFieldName("name");

    private static readonly ValidationKeyDefinition EmailInvalid = ValidationKeyDefinition
        .Create("User.EmailInvalid").WithStringParameter("email").WithFieldName("email");

    private static readonly ValidationKeyDefinition Unmapped = ValidationKeyDefinition
        .Create("Legacy.Locked");

    private static (ResultExceptionHandler Handler, CapturingProblemDetailsService Sink, RecordingLogger Log) Create(
        Action<ResultProblemDetailsOptions>? configure = null, IResultMessageTranslator? translator = null)
    {
        var options = new ResultProblemDetailsOptions();
        configure?.Invoke(options);
        var sink = new CapturingProblemDetailsService();
        var log = new RecordingLogger();
        var handler = new ResultExceptionHandler(sink, translator ?? new KeyResultMessageTranslator(),
            Options.Create(options), log);
        return (handler, sink, log);
    }

    private static DefaultHttpContext Context(string method = "GET", string path = "/api/users/1")
    {
        var ctx = new DefaultHttpContext { TraceIdentifier = "req-1" };
        ctx.Request.Method = method;
        ctx.Request.Path = path;
        return ctx;
    }

    private static ResultException Thrown(Result failed)
    {
        try { failed.ThrowIfFailure(); }
        catch (ResultException e) { return e; }
        throw new InvalidOperationException("expected a failure");
    }

    [Fact]
    public async Task NonResultException_IsNotHandled()
    {
        var (handler, sink, _) = Create();
        var ctx = Context();

        var handled = await handler.TryHandleAsync(ctx, new InvalidOperationException("boom"), CancellationToken.None);

        Assert.False(handled);
        Assert.Null(sink.Captured);
        Assert.Equal(200, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task ResultException_WithoutStatusMetadata_Is400()
    {
        var (handler, sink, _) = Create();
        var ctx = Context("POST", "/api/users");

        var handled = await handler.TryHandleAsync(ctx, Thrown(Result.Error(NameRequired)), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(400, ctx.Response.StatusCode);
        var p = sink.Captured!;
        Assert.Equal(400, p.Status);
        Assert.Equal("Bad Request", p.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", p.Type);
        Assert.Equal("POST /api/users", p.Instance);
        Assert.Equal("req-1", p.Extensions["requestId"]);
        Assert.True(p.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task KeyWithHttpStatus_DrivesStatusAndType()
    {
        var (handler, sink, _) = Create();
        var ctx = Context();

        await handler.TryHandleAsync(ctx, Thrown(Result.Error(NotFound, Guid.NewGuid())), CancellationToken.None);

        Assert.Equal(404, ctx.Response.StatusCode);
        Assert.Equal(404, sink.Captured!.Status);
        Assert.Equal("Not Found", sink.Captured.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", sink.Captured.Type);
    }

    [Fact]
    public async Task KeyDeclaredWithTheEnumForm_IsHonouredToo()
    {
        var conflict = ValidationKeyDefinition.Create("User.EmailTaken").WithHttpStatusCode(System.Net.HttpStatusCode.Conflict);
        var (handler, sink, _) = Create();

        await handler.TryHandleAsync(Context(), Thrown(Result.Error(conflict)), CancellationToken.None);

        Assert.Equal(409, sink.Captured!.Status);
    }

    [Fact]
    public async Task MapStatus_AppliesToKeysWithoutMetadata_ButMetadataWins()
    {
        var (handler, sink, _) = Create(o => o.MapStatus(Unmapped, 423).MapStatus(NotFound, 410));

        await handler.TryHandleAsync(Context(), Thrown(Result.Error(Unmapped)), CancellationToken.None);
        Assert.Equal(423, sink.Captured!.Status);

        await handler.TryHandleAsync(Context(), Thrown(Result.Error(NotFound, Guid.NewGuid())), CancellationToken.None);
        Assert.Equal(404, sink.Captured!.Status);
    }

    [Fact]
    public async Task MixedStatuses_FallBackToDefault()
    {
        var (handler, sink, _) = Create();
        var failed = Result.Create([
            ValidationMessage.Create(NotFound, Guid.NewGuid()),
            ValidationMessage.Create(NameRequired),
        ]);

        await handler.TryHandleAsync(Context(), Thrown(failed), CancellationToken.None);

        Assert.Equal(400, sink.Captured!.Status);
    }

    [Fact]
    public async Task MixedStatuses_WithAServerError_PickTheHighest()
    {
        var upstream = ValidationKeyDefinition.Create("Upstream.Down").WithHttpStatus(503);
        var (handler, sink, _) = Create();
        var failed = Result.Create([ValidationMessage.Create(upstream), ValidationMessage.Create(NameRequired)]);

        await handler.TryHandleAsync(Context(), Thrown(failed), CancellationToken.None);

        Assert.Equal(503, sink.Captured!.Status);
    }

    [Fact]
    public async Task StatusSelector_Overrides()
    {
        var (handler, sink, _) = Create(o => o.StatusSelector = _ => 422);

        await handler.TryHandleAsync(Context(), Thrown(Result.Error(NotFound, Guid.NewGuid())), CancellationToken.None);

        Assert.Equal(422, sink.Captured!.Status);
    }

    [Fact]
    public async Task Detail_IsTranslatedMessagesJoined()
    {
        var (handler, sink, _) = Create(translator: new UpperCaseTranslator());
        var failed = Result.Create([ValidationMessage.Create(NameRequired), ValidationMessage.Create(EmailInvalid, "x")]);

        await handler.TryHandleAsync(Context(), Thrown(failed), CancellationToken.None);

        Assert.Equal("USER.NAMEREQUIRED; USER.EMAILINVALID:x", sink.Captured!.Detail);
    }

    [Fact]
    public async Task EmptyTranslation_FallsBackToKey()
    {
        var (handler, sink, _) = Create(translator: new EmptyTranslator());

        await handler.TryHandleAsync(Context(), Thrown(Result.Error(NameRequired)), CancellationToken.None);

        Assert.Equal("User.NameRequired", sink.Captured!.Detail);
    }

    [Fact]
    public async Task Errors_GroupByFieldName_UnnamedUnderEmptyKey()
    {
        var (handler, sink, _) = Create();
        var failed = Result.Create([
            ValidationMessage.Create(NameRequired),
            ValidationMessage.Create(EmailInvalid, "x"),
            ValidationMessage.Create(EmailInvalid, "y"),
            ValidationMessage.Create(Unmapped),
        ]);

        await handler.TryHandleAsync(Context(), Thrown(failed), CancellationToken.None);

        var errors = Assert.IsType<Dictionary<string, string[]>>(sink.Captured!.Extensions["errors"]);
        Assert.Equal(["User.NameRequired"], errors["name"]);
        Assert.Equal(2, errors["email"].Length);
        Assert.Equal(["Legacy.Locked"], errors[""]);
    }

    [Fact]
    public async Task Validation_IsLossless()
    {
        var id = Guid.NewGuid();
        var (handler, sink, _) = Create();

        await handler.TryHandleAsync(Context(), Thrown(Result.Error(NotFound, id)), CancellationToken.None);

        var entries = Assert.IsType<ValidationProblemEntry[]>(sink.Captured!.Extensions["validation"]);
        var entry = Assert.Single(entries);
        Assert.Equal("User.NotFound", entry.Key);
        Assert.Null(entry.Field);
        Assert.Equal("User.NotFound", entry.Message);
        Assert.Equal(id, entry.Parameters["id"]);
    }

    [Fact]
    public async Task Extensions_CanBeTurnedOff()
    {
        var (handler, sink, _) = Create(o => { o.IncludeErrorsExtension = false; o.IncludeValidationExtension = false; });

        await handler.TryHandleAsync(Context(), Thrown(Result.Error(NameRequired)), CancellationToken.None);

        Assert.False(sink.Captured!.Extensions.ContainsKey("errors"));
        Assert.False(sink.Captured.Extensions.ContainsKey("validation"));
    }

    [Fact]
    public async Task LogsAtInformation_ByDefault_WithKeys()
    {
        var (handler, _, log) = Create();

        await handler.TryHandleAsync(Context(), Thrown(Result.Error(NameRequired)), CancellationToken.None);

        var entry = Assert.Single(log.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Contains("User.NameRequired", entry.Message);
        Assert.Contains("400", entry.Message);
    }

    [Fact]
    public async Task LogLevel_IsConfigurable()
    {
        var (handler, _, log) = Create(o => o.ValidationFailureLogLevel = LogLevel.Warning);

        await handler.TryHandleAsync(Context(), Thrown(Result.Error(NameRequired)), CancellationToken.None);

        Assert.Equal(LogLevel.Warning, Assert.Single(log.Entries).Level);
    }

    [Fact]
    public async Task UnknownStatus_GetsAboutBlankType()
    {
        var teapot = ValidationKeyDefinition.Create("Pot.Short").WithHttpStatus(418);
        var (handler, sink, _) = Create();

        await handler.TryHandleAsync(Context(), Thrown(Result.Error(teapot)), CancellationToken.None);

        Assert.Equal(418, sink.Captured!.Status);
        Assert.Equal("about:blank", sink.Captured.Type);
    }
}

public class HttpStatusMetadataTests
{
    [Fact]
    public void WithHttpStatus_RoundTrips()
    {
        var key = ValidationKeyDefinition.Create("A.B").WithHttpStatus(409);

        Assert.True(key.TryGetHttpStatus(out var status));
        Assert.Equal(409, status);
        Assert.Equal(System.Net.HttpStatusCode.Conflict, key.Metadata[HttpStatusMetadata.Key]);
        Assert.True(key.TryGetHttpStatusCode(out var typed));
        Assert.Equal(System.Net.HttpStatusCode.Conflict, typed);
    }

    [Fact]
    public void WithHttpStatus_RejectsNonHttpNumbers()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ValidationKeyDefinition.Create("A.B").WithHttpStatus(42));
    }

    [Fact]
    public void KeyWithoutStatus_ReportsNone()
    {
        Assert.False(ValidationKeyDefinition.Create("A.B").TryGetHttpStatus(out _));
    }
}
