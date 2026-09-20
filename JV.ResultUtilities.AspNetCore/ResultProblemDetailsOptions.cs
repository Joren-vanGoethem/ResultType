using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;
using Microsoft.Extensions.Logging;

namespace JV.ResultUtilities.AspNetCore;

// Inside JV.ResultUtilities.* the bare name resolves to the ValidationMessage *namespace*; an alias
// declared inside this namespace takes precedence and restores the type.
using ValidationMessage = JV.ResultUtilities.ValidationMessage.ValidationMessage;

/// <summary>
/// Configures how <see cref="ResultExceptionHandler"/> turns a <c>ResultException</c> into a ProblemDetails
/// response. All defaults produce the historical body (400, <c>detail</c> joined with "; ") plus the
/// structured <c>errors</c> and <c>validation</c> extensions.
/// </summary>
public sealed class ResultProblemDetailsOptions
{
    private readonly Dictionary<string, int> _statusByKey = new(StringComparer.Ordinal);

    /// <summary>Status for a message whose key declares none. Default 400.</summary>
    public int DefaultStatusCode { get; set; } = 400;

    /// <summary>
    /// Emit <c>errors</c>: a field-name → messages dictionary in the same shape ASP.NET Core's own
    /// <c>ValidationProblemDetails</c> uses, so a client handles model-binding failures and domain
    /// failures with one code path. Messages whose key has no <c>FieldName</c> go under
    /// <see cref="UnnamedFieldKey"/>. Default true.
    /// </summary>
    public bool IncludeErrorsExtension { get; set; } = true;

    /// <summary>
    /// Emit <c>validation</c>: one <see cref="ValidationProblemEntry"/> per message, with the stable key,
    /// field, translated message and named raw parameters. This is the lossless form. Default true.
    /// </summary>
    public bool IncludeValidationExtension { get; set; } = true;

    /// <summary>Key used in <c>errors</c> for messages without a field name. Default "" (ASP.NET's convention).</summary>
    public string UnnamedFieldKey { get; set; } = string.Empty;

    /// <summary>Separator between translated messages in <c>detail</c>. Default "; ".</summary>
    public string MessageSeparator { get; set; } = "; ";

    /// <summary>
    /// Level at which a handled <c>ResultException</c> is logged. A validation failure is a client
    /// error, not a server fault; default <see cref="LogLevel.Information"/>.
    /// </summary>
    public LogLevel ValidationFailureLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Picks the response status from the per-message statuses (one per message, each resolved from key
    /// metadata, then <see cref="MapStatus"/>, then <see cref="DefaultStatusCode"/>). The default: when
    /// every message agrees, that status; when they disagree and any is a 5xx, the highest; otherwise
    /// <see cref="DefaultStatusCode"/>.
    /// </summary>
    public Func<IReadOnlyList<int>, int>? StatusSelector { get; set; }

    /// <summary>
    /// Maps a key to a status without touching the key's declaration — for keys you do not own, or
    /// when the domain must stay free of HTTP numbers. A status declared on the key itself
    /// (<c>WithHttpStatusCode</c> / <c>WithHttpStatus</c>) wins over this map.
    /// </summary>
    public ResultProblemDetailsOptions MapStatus(ValidationKeyDefinition key, int statusCode)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        _statusByKey[key.Key] = statusCode;
        return this;
    }

    /// <summary>Resolves the status for one message. Exposed for tests and custom handlers.</summary>
    public int ResolveStatus(ValidationMessage message)
    {
        if (message.TryGetHttpStatusCode(out var declared))
            return (int)declared;

        var key = message.KeyDefinition?.Key ?? message.TranslationKey;
        return _statusByKey.TryGetValue(key, out var mapped) ? mapped : DefaultStatusCode;
    }

    /// <summary>Applies <see cref="StatusSelector"/> or the default rule to the per-message statuses.</summary>
    public int SelectStatus(IReadOnlyList<int> statuses)
    {
        if (statuses.Count == 0) return DefaultStatusCode;
        if (StatusSelector != null) return StatusSelector(statuses);

        var first = statuses[0];
        if (statuses.All(s => s == first)) return first;

        var max = statuses.Max();
        return max >= 500 ? max : DefaultStatusCode;
    }
}
