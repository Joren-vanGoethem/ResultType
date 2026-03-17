using JV.ResultUtilities.Demo.Infrastructure;
using JV.ResultUtilities.Demo.Products.Models;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;
using Microsoft.EntityFrameworkCore;

namespace JV.ResultUtilities.Demo.Products;

/// <summary>
/// Wraps EF Core calls and returns Result types.
/// Demonstrates: Result.Ok(), Result.Error(), Result.Try(), implicit operators
/// </summary>
public class ProductRepository(AppDbContext db)
{
    /// <summary>
    /// Demonstrates: Result.Ok(value), Result.Error(key, parameter)
    /// </summary>
    public async Task<Result<Product>> GetByIdAsync(Guid id)
    {
        var product = await db.Products.FindAsync(id);

        if (product is null)
            return Result.Error(ProductValidationKeys.NotFound, id);

        return Result.Ok(product);
    }

    public async Task<Result<List<Product>>> GetAllActiveAsync()
    {
        var products = await db.Products.Where(p => p.IsActive).ToListAsync();
        return Result.Ok(products);
    }

    public async Task<Result<List<Product>>> GetByCategoryAsync(string category)
    {
        var products = await db.Products
            .Where(p => p.IsActive && p.Category.ToString() == category)
            .ToListAsync();
        return Result.Ok(products);
    }

    public async Task<Result<Product?>> FindBySkuAsync(string sku)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Sku == sku);
        return Result.Ok(product);
    }

    /// <summary>
    /// Demonstrates: Result.Try() — wrapping a potentially failing EF Core save
    /// </summary>
    public Result<Product> Add(Product product)
    {
        // Demonstrates: Result.Try() — sync try/catch wrapper
        return Result.Try(() =>
        {
            db.Products.Add(product);
            db.SaveChanges();
            return product;
        }, ProductValidationKeys.SaveFailed);
    }

    /// <summary>
    /// Demonstrates: Result.TryAsync() — async try/catch wrapper
    /// </summary>
    public async Task<Result<Product>> AddAsync(Product product)
    {
        return await Result.TryAsync(async () =>
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return product;
        }, ProductValidationKeys.SaveFailed);

    }

    public async Task<Result<Product>> UpdateAsync(Product product)
    {
        return await Result.TryAsync(async () =>
        {
            product.UpdatedAt = DateTime.UtcNow;
            db.Products.Update(product);
            await db.SaveChangesAsync();
            return product;
        }, ProductValidationKeys.SaveFailed);
    }

    public async Task<Result> SaveChangesAsync()
    {
        // Demonstrates: non-generic Result.TryAsync(Func<Task>, ...)
        return await Result.TryAsync(
            async () => { await db.SaveChangesAsync(); },
            ProductValidationKeys.SaveFailed);
    }
}
