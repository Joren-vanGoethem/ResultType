namespace JV.ResultUtilities.Demo.Products.Models;

public record UpdateProductRequest(
    string? Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    ProductCategory? Category,
    bool IsActive);
