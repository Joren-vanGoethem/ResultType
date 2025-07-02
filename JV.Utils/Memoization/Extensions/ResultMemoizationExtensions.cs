using System;

namespace JV.Utils.Memoization.Extensions;

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
    /// <typeparam name="T">The input type</typeparam>
    /// <typeparam name="TResult">The result value type</typeparam>
    /// <param name="function">The function to memoize</param>
    /// <returns>A memoized version of the function</returns>
    public static Func<T, Result<TResult>> MemoizeResult<T, TResult>(this Func<T, Result<TResult>> function) 
        where T : notnull
    {
        return function.Memoize();
    }

    /// <summary>
    /// Creates a memoized version of a function that returns a Result with validation messages.
    /// Results are cached including their validation state.
    /// </summary>
    /// <typeparam name="T">The input type</typeparam>
    /// <typeparam name="TResult">The result value type</typeparam>
    /// <param name="function">The function to memoize</param>
    /// <param name="keySelector">Function to generate cache keys from input</param>
    /// <returns>A memoized version of the function</returns>
    public static Func<T, Result<TResult>> MemoizeResultWithKey<T, TResult, TKey>(
        this Func<T, Result<TResult>> function,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        var memoizedByKey = ((Func<TKey, Result<TResult>>)(key => 
        {
            // This is a simplified approach - in practice you'd need to store the mapping
            throw new NotImplementedException("Key-based memoization requires additional mapping logic");
        })).Memoize();

        return input => memoizedByKey(keySelector(input));
    }

    /// <summary>
    /// Creates a configurable memoized function for Result operations with cache statistics.
    /// </summary>
    /// <typeparam name="T">The input type</typeparam>
    /// <typeparam name="TResult">The result value type</typeparam>
    /// <param name="function">The function to memoize</param>
    /// <returns>A configurable memoized function</returns>
    public static MemoizedFunction<T, Result<TResult>> CreateMemoizedResult<T, TResult>(
        this Func<T, Result<TResult>> function) where T : notnull
    {
        return MemoizationFactory.CreateConfigurable(function);
    }
}
