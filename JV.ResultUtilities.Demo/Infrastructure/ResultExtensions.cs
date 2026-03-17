using JV.ResultUtilities.Demo.Translations;
using JV.ResultUtilities.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JV.ResultUtilities.Demo.Infrastructure;

public static class ResultActionResultExtensions
{
    public static IActionResult ToActionResult(
        this Result result,
        ITranslator translator)
    {
        return result.Match(
            onSuccess: () => (IActionResult)new NoContentResult(),
            onFailure: messages => new UnprocessableEntityObjectResult(
                translator.ToValidationProblemDetails(messages)));
    }

    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        ITranslator translator,
        Func<T, IActionResult>? onSuccess = null)
    {
        return result.Match(
            onSuccess: value => onSuccess?.Invoke(value) ?? new OkObjectResult(value),
            onFailure: messages => new UnprocessableEntityObjectResult(
                translator.ToValidationProblemDetails(messages)));
    }

    public static async Task<IActionResult> ToActionResultAsync<T>(
        this Task<Result<T>> resultTask,
        ITranslator translator,
        Func<T, IActionResult>? onSuccess = null)
    {
        return await resultTask.MatchAsync(
            onSuccess: value => onSuccess?.Invoke(value) ?? (IActionResult)new OkObjectResult(value),
            onFailure: messages => new UnprocessableEntityObjectResult(
                translator.ToValidationProblemDetails(messages)));
    }

    public static async Task<IActionResult> ToActionResultAsync(
        this Task<Result> resultTask,
        ITranslator translator)
    {
        var result = await resultTask;
        return result.Match(
            onSuccess: () => (IActionResult)new NoContentResult(),
            onFailure: messages => new UnprocessableEntityObjectResult(
                translator.ToValidationProblemDetails(messages)));
    }
}
