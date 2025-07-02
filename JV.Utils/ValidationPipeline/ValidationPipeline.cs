using System;
using System.Collections.Generic;
using System.Linq;
using JV.Utils.Extensions;

namespace JV.Utils.ValidationPipeline;

public class ValidationPipeline<T>
{
    private readonly List<Func<T, Result>> _validators = new();

    public ValidationPipeline<T> AddRule(Func<T, Result> validator)
    {
        _validators.Add(validator);
        return this;
    }

    public Result<T> Validate(T value)
    {
        var results = _validators.Select(v => v(value));
        var mergedResult = results.MergeResults();

        return mergedResult.IsSuccessful
            ? Result.Ok(value)
            : Result.Create<T>(default, mergedResult.ValidationMessages);
    }
}