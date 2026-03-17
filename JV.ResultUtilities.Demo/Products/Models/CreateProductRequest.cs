namespace JV.ResultUtilities.Demo.Products.Models;

public record CreateProductRequest(
    string? Name,
    string? Sku,
    string? Description,
    decimal Price,
    int StockQuantity,
    ProductCategory? Category);
