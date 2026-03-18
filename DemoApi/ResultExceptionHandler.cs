using System.Diagnostics;
using DemoApi.Translations;
using JV.ResultUtilities.Exceptions;
using JV.ResultUtilities.ValidationMessage;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;

namespace DemoApi;

public sealed class ResultExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ITranslator translator,
    ILogger<ResultExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ResultException => StatusCodes.Status400BadRequest,
            ApplicationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        // Let ProblemDetailsService handle Status and ContentType — don't set them on the response directly.
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = StatusCodeTypeUri(statusCode),
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = exception.Message, // dangerous in production
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
        };
        
        var traceId = Activity.Current?.TraceId.ToString();
        problemDetails.Extensions["traceId"] =
            string.IsNullOrWhiteSpace(traceId) ? httpContext.TraceIdentifier : traceId;
        
        problemDetails.Extensions["requestId"] = httpContext.TraceIdentifier;

        if (exception is ResultException resultException)
        {
            logger.LogError("Validation errors: {Errors}",
                string.Join("; ", resultException.ValidationMessages.Select(vm => vm.MapToErrorMessage())));

            // we override the detail field with the translated errors, this gives the user better feedback than the exception message
            problemDetails.Detail =
                string.Join("; ", resultException.ValidationMessages.Select(TranslateValidationMessage));
            
            // ; as delimited because someone might put a , in a translation and then the frontend cannot properly split them
        }

        // In .NET 10, returning true here also suppresses duplicate middleware diagnostics by default.
        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
    }

    private static readonly Dictionary<int, string> StatusCodeTypeUris = new()
    {
        [400] = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        [401] = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        [403] = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        [404] = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        [405] = "https://tools.ietf.org/html/rfc9110#section-15.5.6",
        [409] = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        [422] = "https://tools.ietf.org/html/rfc9110#section-15.5.21",
        [500] = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
    };

    private static string StatusCodeTypeUri(int statusCode) =>
        StatusCodeTypeUris.GetValueOrDefault(statusCode, "about:blank");

    private string TranslateValidationMessage(ValidationMessage validationMessage)
    {
        var key = validationMessage.TranslationKey;
        var translationResult = translator.Translate(key, validationMessage.Parameters.ToArray<object>());
        return string.IsNullOrEmpty(translationResult) ? key : translationResult;
    }
}