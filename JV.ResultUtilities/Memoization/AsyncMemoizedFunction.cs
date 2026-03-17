using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace JV.ResultUtilities.Memoization;

/// <summary>
/// A memoized async function that caches results based on input parameters.
/// </summary>
/// <typeparam name="TKey">The type of the cache key</typeparam>
/// <typeparam name="TResult">The type of the function result</typeparam>
public class AsyncMemoizedFunction<TKey, TResult> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TResult> _cache = new();
    private readonly Func<TKey, Task<TResult>> _function;
    private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks = new();
    private long _hitCount;
    private long _missCount;

    public AsyncMemoizedFunction(Func<TKey, Task<TResult>> function)
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
    /// Invokes the memoized async function with the specified key.
    /// </summary>
    public async Task<TResult> InvokeAsync(TKey key)
    {
        if (_cache.TryGetValue(key, out var cachedResult))
        {
            Interlocked.Increment(ref _hitCount);
            return cachedResult;
        }

        var keyLock = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(key, out cachedResult))
            {
                Interlocked.Increment(ref _hitCount);
                return cachedResult;
            }

            Interlocked.Increment(ref _missCount);
            var result = await _function(key);
            _cache.TryAdd(key, result);
            return result;
        }
        finally
        {
            keyLock.Release();
        }
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
    public bool RemoveFromCache(TKey key)
    {
        return _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    public bool ContainsKey(TKey key)
    {
        return _cache.ContainsKey(key);
    }
}
