using JV.ResultUtilities.Exceptions;
using JV.ResultUtilities.Validation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JV.ResultUtilities.AspNetCore;

/// <summary>
/// Runs the registered <see cref="IValidator"/> for each action argument whose type has one, before
/// the action executes. A failure throws <see cref="ResultException"/>, which
/// <see cref="ResultExceptionHandler"/> turns into the same problem body a domain failure gets — so
/// request-shape errors and business-rule errors are one shape on the wire, and both are translated.
/// </summary>
public sealed class ValidateModelFilter(IEnumerable<IValidator> validators) : IAsyncActionFilter
{
    private readonly Dictionary<Type, IValidator> _validatorMap =
        validators.ToDictionary(v => v.ValidatedType);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            if (!_validatorMap.TryGetValue(argument.GetType(), out var validator))
                continue;

            var result = await validator.ValidateObjectAsync(argument);

            if (result.IsFailure)
                throw new ResultException(result.ValidationMessages);
        }

        await next();
    }
}
