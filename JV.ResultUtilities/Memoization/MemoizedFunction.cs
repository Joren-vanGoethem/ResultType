using System;
using System.Collections.Concurrent;
using System.Threading;

namespace JV.ResultUtilities.Memoization;

/// <summary>
/// Represents a memoized function that caches results based on input parameters.
/// Provides additional functionality like cache management and statistics.
/// </summary>
/// <typeparam name="TKey">The type of the cache key</typeparam>
/// <typeparam name="TResult">The type of the function result</typeparam>
public class MemoizedFunction<TKey, TResult> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TResult> _cache = new();
    private readonly Func<TKey, TResult> _function;
    private long _hitCount;
    private long _missCount;

    /// <summary>
    /// Initializes a new instance of the MemoizedFunction class.
    /// </summary>
    /// <param name="function">The function to memoize</param>
    public MemoizedFunction(Func<TKey, TResult> function)
    {
        _function = function ?? throw new ArgumentNullException(nameof(function));
    }

    /// <summary>
    /// Gets the number of cache hits.
    /// </summary>
    public long HitCount => _hitCount;

    /// <summary>
    /// Gets the number of cache misses.
    /// </summary>
    public long MissCount => _missCount;

    /// <summary>
    /// Gets the total number of cache accesses.
    /// </summary>
    public long TotalAccesses => _hitCount + _missCount;

    /// <summary>
    /// Gets the cache hit ratio as a percentage.
    /// </summary>
    public double HitRatio => TotalAccesses == 0 ? 0 : (double)_hitCount / TotalAccesses * 100;

    /// <summary>
    /// Gets the number of items currently in the cache.
    /// </summary>
    public int CacheSize => _cache.Count;

    /// <summary>
    /// Invokes the memoized function with the specified key.
    /// </summary>
    /// <param name="key">The input parameter</param>
    /// <returns>The cached or computed result</returns>
    public TResult Invoke(TKey key)
    {
        if (_cache.TryGetValue(key, out var cachedResult))
        {
            Interlocked.Increment(ref _hitCount);
            return cachedResult;
        }

        Interlocked.Increment(ref _missCount);
        return _cache.GetOrAdd(key, _function);
    }

    /// <summary>
    /// Clears all cached results and resets statistics.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _hitCount, 0);
        Interlocked.Exchange(ref _missCount, 0);
    }

    /// <summary>
    /// Removes a specific key from the cache.
    /// </summary>
    /// <param name="key">The key to remove</param>
    /// <returns>True if the key was removed, false if it wasn't in the cache</returns>
    public bool RemoveFromCache(TKey key)
    {
        return _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key exists in the cache, false otherwise</returns>
    public bool ContainsKey(TKey key)
    {
        return _cache.ContainsKey(key);
    }
}
