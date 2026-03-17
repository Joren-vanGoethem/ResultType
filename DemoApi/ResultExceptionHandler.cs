using System.Diagnostics;
using DemoApi.Translations;
using JV.ResultUtilities.Exceptions;
using JV.ResultUtilities.ValidationMessage;
using Microsoft.AspNetCore.Diagnostics;
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
            ApplicationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        // Let ProblemDetailsService handle Status and ContentType — don't set them on the response directly.
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = exception.GetType().Name,
            Title = translator.Translate("AnErrorOccured"),
            Detail = exception.Message, // dangerous in production
        };

        if (exception is ResultException resultException)
        {
            logger.LogError("Validation errors: {Errors}",
                string.Join(", ", resultException.ValidationMessages.Select(vm => vm.MapToErrorMessage())));

            // we override the detail field with the translated errors, this gives the user better feedback than the exception message
            problemDetails.Detail =
                string.Join(", ", resultException.ValidationMessages.Select(TranslateValidationMessage));
        }

        var traceId = Activity.Current?.TraceId.ToString();
        problemDetails.Extensions["traceId"] =
            string.IsNullOrWhiteSpace(traceId) ? httpContext.TraceIdentifier : traceId;

        // In .NET 10, returning true here also suppresses duplicate middleware diagnostics by default.
        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
    }

    private string TranslateValidationMessage(ValidationMessage validationMessage)
    {
        var key = validationMessage.TranslationKey;
        var translationResult = translator.Translate(key, validationMessage.Parameters.ToArray<object>());
        return string.IsNullOrEmpty(translationResult) ? key : translationResult;
    }
}