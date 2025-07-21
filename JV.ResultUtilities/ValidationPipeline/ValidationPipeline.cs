// ServiceCruiser is our proprietary software and all source code, databases, functionality, software, website designs, audio, video, text, photographs, graphics (collectively referred to as the ‘Content’) and all intellectual property rights, including all copyright, all trademarks, all logos and all know-how vested therein or related thereto  (collectively referred to as the ‘IPR’) are owned, licenced or controlled by ESAS 3Services NV (or any of its affiliates or subsidiaries), excluded is Content or IPR provided and owned by third parties. All Content and IPR are protected by copyright and trademark laws and various other intellectual property rights legislation and/or other European Union and/or Belgian legislation, including unfair commercial practices legislation.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JV.ResultUtilities.Extensions;

namespace JV.ResultUtilities.ValidationPipeline;

public class ValidationPipeline<T>
{
    private readonly List<Func<T, Result>> _syncValidators = new();
    private readonly List<Func<T, Task<Result>>> _asyncValidators = new();

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
        // Process sync validators without async overhead
        var syncResults = _syncValidators.Select(v => v(value));
        
        // Process async validators concurrently
        var asyncTasks = _asyncValidators.Select(v => v(value));
        var asyncResults = await Task.WhenAll(asyncTasks);
        
        // Merge all results
        var allResults = syncResults.Concat(asyncResults);
        var mergedResult = allResults.MergeResults();

        return mergedResult.IsSuccessful
            ? Result.Ok(value)
            : Result.Error(mergedResult.ValidationMessages);
    }

    public Result<T> Validate(T value)
    {
        // Pure sync execution when no async validators
        if (_asyncValidators.Count == 0)
        {
            var results = _syncValidators.Select(v => v(value));
            var mergedResult = results.MergeResults();
            return mergedResult.IsSuccessful
                ? Result.Ok(value)
                : Result.Error(mergedResult.ValidationMessages);
        }

        // Fall back to async execution
        return ValidateAsync(value).GetAwaiter().GetResult();
    }

}
