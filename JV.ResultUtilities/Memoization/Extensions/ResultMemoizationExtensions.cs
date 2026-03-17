using System;
using System.Collections.Concurrent;

namespace JV.ResultUtilities.Memoization.Extensions;

/// <summary>
/// Extension methods that integrate memoization with the Result type system.
/// These extensions allow you to cache expensive operations that return Result types.
/// </summary>
public static class ResultMemoizationExtensions
{
    /// <summary>
    /// Creates a memoized version of a function that returns a Result.
    /// This is particularly useful for caching validation results or other expensive operations.
    /// </summary>
    public static Func<T, Result<TResult>> MemoizeResult<T, TResult>(this Func<T, Result<TResult>> function)
        where T : notnull
    {
        return function.Memoize();
    }

    /// <summary>
    /// Creates a memoized version of a function that returns a Result, using a custom key selector.
    /// The key selector extracts a cache key from the input, allowing inputs with the same key to share cached results.
    /// </summary>
    public static Func<T, Result<TResult>> MemoizeResultWithKey<T, TResult, TKey>(
        this Func<T, Result<TResult>> function,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        var cache = new ConcurrentDictionary<TKey, Result<TResult>>();

        return input =>
        {
            var key = keySelector(input);
            return cache.GetOrAdd(key, _ => function(input));
        };
    }

    /// <summary>
    /// Creates a configurable memoized function for Result operations with cache statistics.
    /// </summary>
    public static MemoizedFunction<T, Result<TResult>> CreateMemoizedResult<T, TResult>(
        this Func<T, Result<TResult>> function) where T : notnull
    {
        return MemoizationFactory.CreateConfigurable(function);
    }
}
