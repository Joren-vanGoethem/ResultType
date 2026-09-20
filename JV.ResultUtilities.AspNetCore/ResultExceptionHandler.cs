using System.Diagnostics;
using JV.ResultUtilities.Exceptions;
using JV.ResultUtilities.ValidationMessage;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JV.ResultUtilities.AspNetCore;

// Inside JV.ResultUtilities.* the bare name resolves to the ValidationMessage *namespace*; an alias
// declared inside this namespace takes precedence and restores the type.
using ValidationMessage = JV.ResultUtilities.ValidationMessage.ValidationMessage;

/// <summary>
/// Answers a thrown <see cref="ResultException"/> with an RFC 9457 problem body whose status comes from
/// the failing keys, whose <c>detail</c> is the translated messages, and whose <c>errors</c> and
/// <c>validation</c> extensions carry the structured form.
/// <para>
/// It handles <see cref="ResultException"/> only and returns <c>false</c> for everything else, so the
/// framework's own exception handling (logging, metrics, the developer page, the default 500 problem
/// body that carries no exception message) stays in charge of genuine faults. Registration order among
/// handlers therefore does not matter, and no exception message reaches a client through this class.
/// </para>
/// </summary>
public sealed class ResultExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IResultMessageTranslator translator,
    IOptions<ResultProblemDetailsOptions> options,
    ILogger<ResultExceptionHandler> logger) : IExceptionHandler
{
    private static readonly Dictionary<int, string> StatusCodeTypeUris = new()
    {
        [400] = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        [401] = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        [403] = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        [404] = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        [405] = "https://tools.ietf.org/html/rfc9110#section-15.5.6",
        [409] = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        [410] = "https://tools.ietf.org/html/rfc9110#section-15.5.11",
        [412] = "https://tools.ietf.org/html/rfc9110#section-15.5.13",
        [422] = "https://tools.ietf.org/html/rfc9110#section-15.5.21",
        [423] = "https://tools.ietf.org/html/rfc4918#section-11.3",
        [500] = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        [503] = "https://tools.ietf.org/html/rfc9110#section-15.6.4",
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ResultException resultException)
            return false;

        var problem = Build(httpContext, resultException);

        // ExceptionHandlerMiddleware has already set the response to 500, and the ProblemDetails writer
        // copies the response status into an unset ProblemDetails.Status — never the other way round.
        // Setting it here is what puts the chosen status on the wire.
        httpContext.Response.StatusCode = problem.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }

    /// <summary>
    /// Builds the problem body without writing it. Public so a controller that must answer a failure
    /// itself (imperative authorization, a proxy) can produce the identical shape.
    /// </summary>
    public ProblemDetails Build(HttpContext httpContext, ResultException exception)
    {
        var opts = options.Value;
        var messages = exception.ValidationMessages;

        var statuses = messages.Select(opts.ResolveStatus).ToArray();
        var statusCode = opts.SelectStatus(statuses);

        var translated = messages.Select(Translate).ToArray();

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Type = StatusCodeTypeUris.GetValueOrDefault(statusCode, "about:blank"),
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = string.Join(opts.MessageSeparator, translated),
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
        };

        var traceId = Activity.Current?.TraceId.ToString();
        problem.Extensions["traceId"] = string.IsNullOrWhiteSpace(traceId) ? httpContext.TraceIdentifier : traceId;
        problem.Extensions["requestId"] = httpContext.TraceIdentifier;

        if (opts.IncludeErrorsExtension)
            problem.Extensions["errors"] = BuildErrors(messages, translated, opts.UnnamedFieldKey);

        if (opts.IncludeValidationExtension)
            problem.Extensions["validation"] = BuildValidation(messages, translated);

        logger.Log(opts.ValidationFailureLogLevel,
            "Request {Method} {Path} rejected with {StatusCode}: {ValidationKeys}",
            httpContext.Request.Method, httpContext.Request.Path, statusCode, exception.Keys);

        return problem;
    }

    private string Translate(ValidationMessage message)
    {
        var text = translator.Translate(message);
        return string.IsNullOrEmpty(text) ? message.TranslationKey : text;
    }

    private static Dictionary<string, string[]> BuildErrors(
        IReadOnlyList<ValidationMessage> messages, string[] translated, string unnamedKey)
    {
        var grouped = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        for (var i = 0; i < messages.Count; i++)
        {
            var field = messages[i].KeyDefinition?.FieldName ?? unnamedKey;
            if (!grouped.TryGetValue(field, out var list))
                grouped[field] = list = [];
            list.Add(translated[i]);
        }

        return grouped.ToDictionary(g => g.Key, g => g.Value.ToArray(), StringComparer.Ordinal);
    }

    private static ValidationProblemEntry[] BuildValidation(
        IReadOnlyList<ValidationMessage> messages, string[] translated)
    {
        var entries = new ValidationProblemEntry[messages.Count];
        for (var i = 0; i < messages.Count; i++)
        {
            var m = messages[i];
            entries[i] = new ValidationProblemEntry(
                m.TranslationKey,
                m.KeyDefinition?.FieldName,
                translated[i],
                m.NamedParameters);
        }

        return entries;
    }
}
