using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JV.Utils.Extensions;

public static class ResultCollectionExtensions
{
    // Transform a collection where any failure fails the entire operation
    public static Result<IEnumerable<TResult>> TraverseAll<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Result<TResult>> transform)
    {
        var results = new List<TResult>();
        var errors = new List<ValidationMessage>();

        foreach (var item in source)
        {
            var result = transform(item);
            if (result.IsSuccessful)
                results.Add(result.Value);
            else
                errors.AddRange(result.ValidationMessages);
        }

        return errors.Any()
            ? Result.Create<IEnumerable<TResult>>(null, errors)
            : Result.Ok<IEnumerable<TResult>>(results);
    }

    public static async Task<Result<IEnumerable<TResult>>> TraverseAllAsync<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Task<Result<TResult>>> transform)
    {
        var results = new List<TResult>();
        var errors = new List<ValidationMessage>();

        foreach (var item in source)
        {
            var result = await transform(item);
            if (result.IsSuccessful)
                results.Add(result.Value);
            else
                errors.AddRange(result.ValidationMessages);
        }

        return errors.Any()
            ? Result.Create<IEnumerable<TResult>>(null, errors)
            : Result.Ok<IEnumerable<TResult>>(results);
    }

    // Transform a collection keeping only successful results
    public static Result<IEnumerable<TResult>> TraversePartial<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Result<TResult>> transform)
    {
        var results = source
            .Select(transform)
            .Where(r => r.IsSuccessful)
            .Select(r => r.Value);

        return Result.Ok(results);
    }

    // Transform a collection keeping only successful results
    public static async Task<Result<IEnumerable<TResult>>> TraversePartialAsync<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Task<Result<TResult>>> transform)
    {
        var tasks = await Task.WhenAll(source.Select(t => transform(t)));

        var inputs = tasks
            .Where(result => result.IsSuccessful)
            .Select(r => r.Value);

        return Result.Ok(inputs);
    }
}