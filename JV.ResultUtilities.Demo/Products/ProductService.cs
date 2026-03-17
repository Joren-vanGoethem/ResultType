using JV.ResultUtilities.Demo.Products.Models;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.Demo.Products;

/// <summary>
/// Business logic for Products. Demonstrates the majority of sync Result operations.
/// </summary>
public class ProductService(
    ProductRepository repository,
    ProductCatalogCache cache,
    ILogger<ProductService> logger)
{
    /// <summary>
    /// Get all active products.
    /// Demonstrates: Map (entity list -> response list)
    /// </summary>
    public async Task<Result<List<ProductResponse>>> GetAllAsync()
    {
        return (await repository.GetAllActiveAsync())
            .Map(products => products
                .Select(ToResponse)
                .ToList());
    }

    /// <summary>
    /// Get product by ID with business rule validation.
    /// Demonstrates: Ensure, Map, Deconstruct (3-value)
    /// </summary>
    public async Task<Result<ProductResponse>> GetByIdAsync(Guid id)
    {
        var result = await repository.GetByIdAsync(id);

        // Demonstrates: Deconstruct into (bool, messages, value)
        var (isSuccess, messages, product) = result;
        if (!isSuccess)
        {
            return Result.Create(messages);
        }

        return Result.Ok(product)
            .Ensure(p => p.IsActive,                      // Ensure: guard business rule
                ProductValidationKeys.Inactive, product.Name)
            .Map(ToResponse);                             // Map: entity -> DTO
    }

    /// <summary>
    /// Create a product with full validation pipeline.
    /// Demonstrates: ValidationPipeline, Bind (check SKU uniqueness), Map, Do (side-effect logging),
    /// implicit conversion (returning bare value as Result&lt;T&gt;)
    /// </summary>
    public async Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request)
    {
        var pipeline = ProductValidator.CreatePipeline();

        // Demonstrates: ValidationPipeline.Validate() — sync validation
        var validationResult = pipeline.Validate(request);
        if (validationResult.IsFailure)
        {
            // Demonstrates: Cast<T> — forward failed result to a different type
            return validationResult.Cast<ProductResponse>();
        }

        // Demonstrates: Bind — chain a check that returns Result
        return Result.Ok(request)
            .Bind(req => CheckSkuUniqueness(req).GetAwaiter().GetResult())
            .Map(ToEntity)
            .Do(p => logger.LogInformation("Creating product {Name} with SKU {Sku}", p.Name, p.Sku))
            .Bind(p => repository.Add(p))             // Result.Try inside repository
            .Do(p => cache.InvalidateCategory(p.Category.ToString()))
            .Map(ToResponse);                          // Map: entity -> response DTO
    }

    /// <summary>
    /// Update a product.
    /// Demonstrates: BindAsync, Merge on Result&lt;T&gt;, MapAsync
    /// </summary>
    public async Task<Result<ProductResponse>> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var productResult = await repository.GetByIdAsync(id);

        return await productResult
            .Do(product => cache.InvalidateCategory(product.Category.ToString())) // invalidate OLD category
            .Map(product =>
            {
                product.Name = request.Name!;
                product.Description = request.Description!;
                product.Price = request.Price;
                product.StockQuantity = request.StockQuantity;
                product.Category = request.Category!.Value;
                product.IsActive = request.IsActive;
                return product;
            })
            .BindAsync(async product => await repository.UpdateAsync(product))
            .MapAsync(async product =>
            {
                cache.InvalidateCategory(product.Category.ToString()); // invalidate NEW category
                return ToResponse(product);
            });
    }

    /// <summary>
    /// Soft-delete a product.
    /// Demonstrates: OnSuccess, Deconstruct (2-value), non-generic Result
    /// </summary>
    public async Task<Result> DeactivateAsync(Guid id)
    {
        var result = await repository.GetByIdAsync(id);

        // Demonstrates: Deconstruct into (Result, value)
        var (validationResult, product) = result;

        // Demonstrates: OnSuccess — chain next operation only if previous succeeded
        return validationResult.OnSuccess(() =>
        {
            product.IsActive = false;
            cache.InvalidateCategory(product.Category.ToString());
            return repository.SaveChangesAsync().GetAwaiter().GetResult();
        });
    }

    /// <summary>
    /// Bulk import products — keeps valid ones, skips invalid.
    /// Demonstrates: TraversePartial (keep only successes, discard failures)
    /// </summary>
    public Result<IEnumerable<ProductResponse>> BulkImport(List<CreateProductRequest> requests)
    {
        var pipeline = ProductValidator.CreatePipeline();

        // Demonstrates: TraversePartial — returns only the valid results, skips failures
        // Note: TraversePartialWithErrors is also available to get both successes and skipped errors
        return requests.TraversePartial(req =>
            pipeline.Validate(req)
                .Map(ToEntity)
                .Bind(p => repository.Add(p))  // persist valid products
                .Map(ToResponse));
    }

    /// <summary>
    /// Validate a batch of products — all must pass.
    /// Demonstrates: TraverseAll (all-or-nothing: collects ALL errors if any fail)
    /// </summary>
    public Result<IEnumerable<CreateProductRequest>> ValidateBatch(List<CreateProductRequest> requests)
    {
        var pipeline = ProductValidator.CreatePipeline();

        // Demonstrates: TraverseAll — processes every item, returns all errors combined
        return requests.TraverseAll(req => pipeline.Validate(req));
    }

    /// <summary>
    /// Get products by category using memoization cache.
    /// Demonstrates: MemoizedFunction cache usage with statistics
    /// </summary>
    public Result<List<ProductResponse>> GetByCategoryCached(string category)
    {
        return cache.GetByCategory(category)
            .Map(products => products.Select(ToResponse).ToList());
    }

    /// <summary>
    /// Get cache statistics.
    /// Demonstrates: MemoizedFunction.HitCount, MissCount, HitRatio, CacheSize
    /// </summary>
    public ProductCatalogCache.CacheStats GetCacheStats() => cache.GetCategoryLookupStats();

    private async Task<Result<CreateProductRequest>> CheckSkuUniqueness(CreateProductRequest request)
    {
        var existing = await repository.FindBySkuAsync(request.Sku);
        if (existing.IsFailure)
            return Result.Ok(request); // lookup failed, no conflict

        return existing.Value is not null
            ? Result.Error<CreateProductRequest>(ProductValidationKeys.SkuDuplicate, request.Sku)
            : Result.Ok(request);
    }

    private static Product ToEntity(CreateProductRequest request) => new()
    {
        Id = Guid.NewGuid(),
        Name = request.Name!,
        Sku = request.Sku!,
        Description = request.Description!,
        Price = request.Price,
        StockQuantity = request.StockQuantity,
        Category = request.Category!.Value,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static ProductResponse ToResponse(Product p) => new(
        p.Id, p.Name, p.Sku, p.Description, p.Price,
        p.StockQuantity, p.Category, p.IsActive, p.CreatedAt);
}
