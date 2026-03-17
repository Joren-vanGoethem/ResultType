using JV.ResultUtilities.Demo.Infrastructure;
using JV.ResultUtilities.Demo.Products.Models;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.Memoization;
using JV.ResultUtilities.Memoization.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JV.ResultUtilities.Demo.Products;

/// <summary>
/// Caches product lookups using the library's memoization subsystem.
/// Demonstrates: MemoizedFunction with cache stats, MemoizationFactory.CreateMemoized() with
/// expiration/size limits, .MemoizeResult(), .CreateMemoizedResult()
/// </summary>
public class ProductCatalogCache
{
    // Demonstrates: .CreateMemoizedResult() — returns MemoizedFunction<T, Result<TResult>> with stats
    private readonly MemoizedFunction<string, Result<List<Product>>> _categoryLookup;

    // Demonstrates: .MemoizeResult() — simple memoized Func returning Result<T>
    private readonly Func<string, Result<List<Product>>> _skuPrefixLookup;

    // Demonstrates: MemoizationFactory.CreateMemoized() with expiration and max cache size
    private readonly Func<Guid, Result<Product>> _productByIdCache;

    // Demonstrates: .Memoize() extension on a pure function (2-arg overload)
    private static readonly Func<decimal, int, decimal> CalculateBulkDiscount =
        ((Func<decimal, int, decimal>)((price, qty) =>
            qty >= 100 ? price * 0.80m :
            qty >= 50 ? price * 0.90m :
            qty >= 10 ? price * 0.95m : price
        )).Memoize();

    public ProductCatalogCache(IServiceScopeFactory scopeFactory)
    {
        // CreateMemoizedResult: configurable cache with hit/miss statistics
        Func<string, Result<List<Product>>> categoryFn = category =>
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var products = db.Products.Where(p => p.IsActive && p.Category.ToString() == category).ToList();
            return Result.Ok(products);
        };
        _categoryLookup = categoryFn.CreateMemoizedResult();

        // MemoizeResult: simple result memoization (thin wrapper over .Memoize())
        Func<string, Result<List<Product>>> skuPrefixFn = prefix =>
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var products = db.Products
                .Where(p => p.IsActive && p.Sku.StartsWith(prefix))
                .ToList();
            return Result.Ok(products);
        };
        _skuPrefixLookup = skuPrefixFn.MemoizeResult();

        // MemoizationFactory.CreateMemoized: bare Func with TTL and size limits
        _productByIdCache = MemoizationFactory.CreateMemoized<Guid, Result<Product>>(
            id =>
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var product = db.Products.Find(id);
                return product is not null
                    ? Result.Ok(product)
                    : Result.Error(ProductValidationKeys.NotFound, id);
            },
            maxCacheSize: 200,
            expiration: TimeSpan.FromMinutes(5));

        // ExpiringMemoizedFunction is now public with hit/miss stats.
        // MemoizeAsync is now available for async memoization.
    }

    public Result<List<Product>> GetByCategory(string category) => _categoryLookup.Invoke(category);
    public Result<List<Product>> GetBySkuPrefix(string prefix) => _skuPrefixLookup(prefix);
    public Result<Product> GetProductById(Guid id) => _productByIdCache(id);
    public static decimal GetBulkDiscount(decimal price, int quantity) => CalculateBulkDiscount(price, quantity);

    // Expose cache stats for monitoring endpoint
    public CacheStats GetCategoryLookupStats() => new(
        _categoryLookup.HitCount,
        _categoryLookup.MissCount,
        _categoryLookup.HitRatio,
        _categoryLookup.CacheSize);

    public void InvalidateCategory(string category) => _categoryLookup.RemoveFromCache(category);
    public void ClearAll() => _categoryLookup.ClearCache();

    public record CacheStats(long Hits, long Misses, double HitRatio, int Size);
}
