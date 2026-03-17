using JV.ResultUtilities.Demo.Products.Models;
using JV.ResultUtilities.ValidationMessage;
using JV.ResultUtilities.ValidationPipeline;

namespace JV.ResultUtilities.Demo.Products;

/// <summary>
/// Validates CreateProductRequest using ValidationPipeline with sync rules only.
/// Demonstrates: ValidationPipeline&lt;T&gt;, AddRule (sync), Validate/ValidateAsync,
/// Result.Ok(), Result.Error() with parameters, ValidationMessage.Create()
/// </summary>
public static class ProductValidator
{
    public static ValidationPipeline<CreateProductRequest> CreatePipeline()
    {
        return new ValidationPipeline<CreateProductRequest>()
            .AddRule(ValidateName)
            .AddRule(ValidatePrice)
            .AddRule(ValidateStock)
            .AddRule(ValidateSku)
            .AddRule(ValidateCategory);
    }

    private static Result ValidateName(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Error(ProductValidationKeys.NameRequired);

        if (request.Name.Length < 3 || request.Name.Length > 100)
            return Result.Error(ProductValidationKeys.NameLength,
                new object[] { request.Name, 3, 100 });

        return Result.Ok();
    }

    private static Result ValidatePrice(CreateProductRequest request)
    {
        return request.Price > 0
            ? Result.Ok()
            : Result.Error(ProductValidationKeys.PriceInvalid, 0m);
    }

    private static Result ValidateStock(CreateProductRequest request)
    {
        return request.StockQuantity >= 0
            ? Result.Ok()
            : Result.Error(ProductValidationKeys.StockNegative);
    }

    private static Result ValidateSku(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sku))
            return Result.Error(ProductValidationKeys.SkuRequired);

        return Result.Ok();
    }

    private static Result ValidateCategory(CreateProductRequest request)
    {
        if (request.Category is null || !Enum.IsDefined(request.Category.Value))
            return Result.Error(ProductValidationKeys.CategoryInvalid,
                request.Category?.ToString() ?? "");

        return Result.Ok();
    }
}
