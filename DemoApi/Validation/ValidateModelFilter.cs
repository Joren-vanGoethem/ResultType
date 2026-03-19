using JV.ResultUtilities.Exceptions;
using JV.ResultUtilities.FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DemoApi.Validation;

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
