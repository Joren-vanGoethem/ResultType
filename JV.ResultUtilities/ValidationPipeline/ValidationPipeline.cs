using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JV.ResultUtilities.Extensions;

namespace JV.ResultUtilities.ValidationPipeline;

public class ValidationPipeline<T>
{
    private readonly List<Func<T, Result>> _syncValidators = new();
    private readonly List<Func<T, Task<Result>>> _asyncValidators = new();

    /// <summary>
    /// When true, validation stops on the first failure instead of collecting all errors.
    /// </summary>
    public bool ShortCircuit { get; set; }

    public ValidationPipeline<T> AddRule(Func<T, Result> validator)
    {
        _syncValidators.Add(validator);
        return this;
    }

    public ValidationPipeline<T> AddRule(Func<T, Task<Result>> validator)
    {
        _asyncValidators.Add(validator);
        return this;
    }

    public async Task<Result<T>> ValidateAsync(T value)
    {
        var allMessages = new List<ValidationMessage.ValidationMessage>();

        // Process sync validators
        foreach (var validator in _syncValidators)
        {
            var result = validator(value);
            if (result.IsFailure)
            {
                allMessages.AddRange(result.ValidationMessages);
                if (ShortCircuit)
                    return Result.Error(allMessages);
            }
        }

        // Process async validators
        if (ShortCircuit)
        {
            foreach (var validator in _asyncValidators)
            {
                var result = await validator(value);
                if (result.IsFailure)
                {
                    allMessages.AddRange(result.ValidationMessages);
                    return Result.Error(allMessages);
                }
            }
        }
        else
        {
            var asyncTasks = _asyncValidators.Select(v => v(value));
            var asyncResults = await Task.WhenAll(asyncTasks);
            foreach (var result in asyncResults)
            {
                if (result.IsFailure)
                    allMessages.AddRange(result.ValidationMessages);
            }
        }

        return allMessages.Count > 0
            ? Result.Error(allMessages)
            : Result.Ok(value);
    }

    /// <summary>
    /// Validates synchronously. Throws <see cref="InvalidOperationException"/> if async rules have been added.
    /// Use <see cref="ValidateAsync"/> when async rules are present.
    /// </summary>
    public Result<T> Validate(T value)
    {
        if (_asyncValidators.Count != 0)
            throw new InvalidOperationException(
                "Cannot use synchronous Validate() when async rules are registered. Use ValidateAsync() instead.");

        if (ShortCircuit)
        {
            var allMessages = new List<ValidationMessage.ValidationMessage>();
            foreach (var validator in _syncValidators)
            {
                var result = validator(value);
                if (result.IsFailure)
                {
                    allMessages.AddRange(result.ValidationMessages);
                    return Result.Error(allMessages);
                }
            }
            return allMessages.Count > 0
                ? Result.Error(allMessages)
                : Result.Ok(value);
        }

        var results = _syncValidators.Select(v => v(value));
        var mergedResult = results.MergeResults();
        return mergedResult.IsSuccessful
            ? Result.Ok(value)
            : Result.Error(mergedResult.ValidationMessages);
    }
}
