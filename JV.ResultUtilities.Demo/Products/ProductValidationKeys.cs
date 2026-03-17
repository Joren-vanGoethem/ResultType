using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.Demo.Products;

/// <summary>
/// All ValidationKeyDefinitions for the Product domain.
/// Demonstrates: ValidationKeyDefinition.Create(), WithStringParameter, WithIntParameter,
/// WithDecimalParameter, WithEnumParameter — using the fluent builder pattern.
/// </summary>
public static class ProductValidationKeys
{
    public static readonly ValidationKeyDefinition NameRequired =
        ValidationKeyDefinition.Create("product.name.required");

    public static readonly ValidationKeyDefinition NameLength =
        ValidationKeyDefinition.Create("product.name.length")
            .WithStringParameter("name")
            .WithIntParameter("minLength")
            .WithIntParameter("maxLength");

    public static readonly ValidationKeyDefinition SkuRequired =
        ValidationKeyDefinition.Create("product.sku.required");

    public static readonly ValidationKeyDefinition SkuDuplicate =
        ValidationKeyDefinition.Create("product.sku.duplicate")
            .WithStringParameter("sku");

    public static readonly ValidationKeyDefinition PriceInvalid =
        ValidationKeyDefinition.Create("product.price.invalid")
            .WithDecimalParameter("minPrice");

    // WithEnumParameter now accepts both Enum instances and non-empty strings
    public static readonly ValidationKeyDefinition CategoryInvalid =
        ValidationKeyDefinition.Create("product.category.invalid")
            .WithEnumParameter("category");

    public static readonly ValidationKeyDefinition StockNegative =
        ValidationKeyDefinition.Create("product.stock.negative");

    public static readonly ValidationKeyDefinition NotFound =
        ValidationKeyDefinition.Create("product.not_found")
            .WithGuidParameter("id");

    public static readonly ValidationKeyDefinition Inactive =
        ValidationKeyDefinition.Create("product.inactive")
            .WithStringParameter("name");

    public static readonly ValidationKeyDefinition SaveFailed =
        ValidationKeyDefinition.Create("product.save_failed")
            .WithStringParameter("error");
}
