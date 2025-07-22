using System;

namespace JV.ResultUtilities.Memoization;

/// <summary>
/// Factory class for creating memoized functions with additional configuration options.
/// </summary>
public static class MemoizationFactory
{
    /// <summary>
    /// Creates a memoized function with cache size limit and optional expiration.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key</typeparam>
    /// <typeparam name="TResult">The type of the function result</typeparam>
    /// <param name="function">The function to memoize</param>
    /// <param name="maxCacheSize">Maximum number of items to keep in cache (optional)</param>
    /// <param name="expiration">Cache item expiration time (optional)</param>
    /// <returns>A memoized function with the specified configuration</returns>
    public static Func<TKey, TResult> CreateMemoized<TKey, TResult>(
        Func<TKey, TResult> function,
        int? maxCacheSize = null,
        TimeSpan? expiration = null) where TKey : notnull
    {
        if (maxCacheSize.HasValue || expiration.HasValue)
        {
            return new ExpiringMemoizedFunction<TKey, TResult>(function, maxCacheSize, expiration).Invoke;
        }

        return new MemoizedFunction<TKey, TResult>(function).Invoke;
    }

    /// <summary>
    /// Creates a memoized function that can be configured with custom cache behavior.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key</typeparam>
    /// <typeparam name="TResult">The type of the function result</typeparam>
    /// <param name="function">The function to memoize</param>
    /// <returns>A configurable memoized function</returns>
    public static MemoizedFunction<TKey, TResult> CreateConfigurable<TKey, TResult>(
        Func<TKey, TResult> function) where TKey : notnull
    {
        return new MemoizedFunction<TKey, TResult>(function);
    }
}
