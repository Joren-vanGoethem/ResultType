using Microsoft.AspNetCore.Mvc;

namespace JV.ResultUtilities.AspNetCore;

/// <summary>
/// One failed validation message as it appears in the <c>validation</c> extension of the problem body.
/// </summary>
/// <param name="Key">The stable translation key, e.g. <c>Backtest.NotFound</c>. Branch on this, never on <paramref name="Message"/>.</param>
/// <param name="Field">The form field the key was declared for (<c>ValidationKeyDefinition.FieldName</c>), or null.</param>
/// <param name="Message">The translated, human-readable text.</param>
/// <param name="Parameters">Raw parameter values keyed by the names declared on the key.</param>
public sealed record ValidationProblemEntry(
    string Key,
    string? Field,
    string Message,
    IReadOnlyDictionary<string, object?> Parameters);

/// <summary>
/// The wire shape <see cref="ResultExceptionHandler"/> writes, for documentation and OpenAPI typing:
/// <c>[ProducesResponseType&lt;ResultProblemDetails&gt;(400)]</c>. The handler itself writes a plain
/// <see cref="ProblemDetails"/> and fills <see cref="ProblemDetails.Extensions"/>, because the
/// framework's ProblemDetails writer serialises the base type; the properties here mirror those
/// extension members one-to-one.
/// </summary>
public class ResultProblemDetails : ProblemDetails
{
    /// <summary>Server-side trace id (the W3C trace id when an Activity is current, else the request id).</summary>
    public string? TraceId { get; set; }

    /// <summary>ASP.NET Core's request identifier.</summary>
    public string? RequestId { get; set; }

    /// <summary>Field name → translated messages, in ASP.NET's <c>ValidationProblemDetails</c> shape.</summary>
    public IDictionary<string, string[]>? Errors { get; set; }

    /// <summary>One entry per validation message, lossless.</summary>
    public IReadOnlyList<ValidationProblemEntry>? Validation { get; set; }
}
