
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace JV.Utils.Memoization;

/// <summary>
/// A memoized function that supports cache expiration and size limits.
/// </summary>
/// <typeparam name="TKey">The type of the cache key</typeparam>
/// <typeparam name="TResult">The type of the function result</typeparam>
internal class ExpiringMemoizedFunction<TKey, TResult> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, CacheEntry> _cache = new();
    private readonly Func<TKey, TResult> _function;
    private readonly int? _maxCacheSize;
    private readonly TimeSpan? _expiration;
    private readonly object _cleanupLock = new();
    private DateTime _lastCleanup = DateTime.UtcNow;

    public ExpiringMemoizedFunction(Func<TKey, TResult> function, int? maxCacheSize, TimeSpan? expiration)
    {
        _function = function ?? throw new ArgumentNullException(nameof(function));
        _maxCacheSize = maxCacheSize;
        _expiration = expiration;
    }

    public TResult Invoke(TKey key)
    {
        CleanupIfNeeded();

        if (_cache.TryGetValue(key, out var entry))
        {
            if (!IsExpired(entry))
            {
                entry.LastAccessed = DateTime.UtcNow;
                return entry.Value;
            }
            
            _cache.TryRemove(key, out _);
        }

        var result = _function(key);
        var newEntry = new CacheEntry(result, DateTime.UtcNow);
        
        _cache.AddOrUpdate(key, newEntry, (_, _) => newEntry);
        
        EnforceSizeLimit();
        
        return result;
    }

    private void CleanupIfNeeded()
    {
        if (!_expiration.HasValue) return;

        var now = DateTime.UtcNow;
        if (now - _lastCleanup < TimeSpan.FromMinutes(1)) return;

        lock (_cleanupLock)
        {
            if (now - _lastCleanup < TimeSpan.FromMinutes(1)) return;

            var expiredKeys = _cache
                .Where(kvp => IsExpired(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }

            _lastCleanup = now;
        }
    }

    private bool IsExpired(CacheEntry entry)
    {
        return _expiration.HasValue && DateTime.UtcNow - entry.CreatedAt > _expiration.Value;
    }

    private void EnforceSizeLimit()
    {
        if (!_maxCacheSize.HasValue || _cache.Count <= _maxCacheSize.Value) return;

        var oldestEntries = _cache
            .OrderBy(kvp => kvp.Value.LastAccessed)
            .Take(_cache.Count - _maxCacheSize.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in oldestEntries)
        {
            _cache.TryRemove(key, out _);
        }
    }

    private class CacheEntry
    {
        public CacheEntry(TResult value, DateTime createdAt)
        {
            Value = value;
            CreatedAt = createdAt;
            LastAccessed = createdAt;
        }

        public TResult Value { get; }
        public DateTime CreatedAt { get; }
        public DateTime LastAccessed { get; set; }
    }
}
