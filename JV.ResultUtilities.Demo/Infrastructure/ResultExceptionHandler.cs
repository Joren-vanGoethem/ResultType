using JV.ResultUtilities.Demo.Translations;
using JV.ResultUtilities.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace JV.ResultUtilities.Demo.Infrastructure;

/// <summary>
/// Catches ResultException thrown by ThrowIfFailure() and converts to ProblemDetails.
/// </summary>
public class ResultExceptionHandler(ITranslator translator)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ResultException resultException)
            return false;

        var problemDetails = translator.ToValidationProblemDetails(resultException.ValidationMessages);

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
