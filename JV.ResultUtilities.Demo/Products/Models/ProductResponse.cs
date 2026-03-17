namespace JV.ResultUtilities.Demo.Products.Models;

public record ProductResponse(
    Guid Id,
    string Name,
    string Sku,
    string Description,
    decimal Price,
    int StockQuantity,
    ProductCategory Category,
    bool IsActive,
    DateTime CreatedAt);
