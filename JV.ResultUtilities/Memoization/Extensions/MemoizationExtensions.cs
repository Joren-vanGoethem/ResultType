using System;
using System.Collections.Concurrent;
using System.Threading;

namespace JV.ResultUtilities.Memoization.Extensions;

/// <summary>
/// Provides extension methods for memoization (caching) of function results.
/// Memoization is an optimization technique that stores the results of expensive function calls
/// and returns the cached result when the same inputs occur again.
/// </summary>
public static class MemoizationExtensions
{
    /// <summary>
    /// Creates a memoized version of a function with no parameters.
    /// The result will be computed only once and cached for subsequent calls.
    /// </summary>
    /// <typeparam name="TResult">The type of the function result</typeparam>
    /// <param name="func">The function to memoize</param>
    /// <returns>A memoized version of the function</returns>
    public static Func<TResult> Memoize<TResult>(this Func<TResult> func)
    {
        var cache = new Lazy<TResult>(func, LazyThreadSafetyMode.ExecutionAndPublication);
        return () => cache.Value;
    }

    /// <summary>
    /// Creates a memoized version of a function with one parameter.
    /// Results are cached based on the input parameter value.
    /// </summary>
    /// <typeparam name="T">The type of the function parameter</typeparam>
    /// <typeparam name="TResult">The type of the function result</typeparam>
    /// <param name="func">The function to memoize</param>
    /// <returns>A memoized version of the function</returns>
    public static Func<T, TResult> Memoize<T, TResult>(this Func<T, TResult> func) where T : notnull
    {
        var cache = new ConcurrentDictionary<T, TResult>();
        return input => cache.GetOrAdd(input, func);
    }

    /// <summary>
    /// Creates a memoized version of a function with two parameters.
    /// Results are cached based on both input parameter values.
    /// </summary>
    /// <typeparam name="T1">The type of the first function parameter</typeparam>
    /// <typeparam name="T2">The type of the second function parameter</typeparam>
    /// <typeparam name="TResult">The type of the function result</typeparam>
    /// <param name="func">The function to memoize</param>
    /// <returns>A memoized version of the function</returns>
    public static Func<T1, T2, TResult> Memoize<T1, T2, TResult>(this Func<T1, T2, TResult> func) 
        where T1 : notnull where T2 : notnull
    {
        var cache = new ConcurrentDictionary<(T1, T2), TResult>();
        return (input1, input2) => cache.GetOrAdd((input1, input2), key => func(key.Item1, key.Item2));
    }

    /// <summary>
    /// Creates a memoized version of a function with three parameters.
    /// Results are cached based on all input parameter values.
    /// </summary>
    /// <typeparam name="T1">The type of the first function parameter</typeparam>
    /// <typeparam name="T2">The type of the second function parameter</typeparam>
    /// <typeparam name="T3">The type of the third function parameter</typeparam>
    /// <typeparam name="TResult">The type of the function result</typeparam>
    /// <param name="func">The function to memoize</param>
    /// <returns>A memoized version of the function</returns>
    public static Func<T1, T2, T3, TResult> Memoize<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func) 
        where T1 : notnull where T2 : notnull where T3 : notnull
    {
        var cache = new ConcurrentDictionary<(T1, T2, T3), TResult>();
        return (input1, input2, input3) => cache.GetOrAdd((input1, input2, input3), key => func(key.Item1, key.Item2, key.Item3));
    }
}
