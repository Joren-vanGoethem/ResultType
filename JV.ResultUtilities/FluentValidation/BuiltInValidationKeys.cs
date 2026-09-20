using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.FluentValidation;

/// <summary>
/// Default ValidationKeyDefinitions for built-in validation rules.
/// Consumers should include translations for these keys (e.g., "Validation.NotEmpty") in their .resx files.
/// </summary>
public static class BuiltInValidationKeys
{
    public static readonly ValidationKeyDefinition NotNull =
        ValidationKeyDefinition.Create("Validation.NotNull")
            .WithStringParameter("FieldName");

    public static readonly ValidationKeyDefinition NotEmpty =
        ValidationKeyDefinition.Create("Validation.NotEmpty")
            .WithStringParameter("FieldName");

    public static readonly ValidationKeyDefinition MaxLength =
        ValidationKeyDefinition.Create("Validation.MaxLength")
            .WithStringParameter("FieldName")
            .WithIntParameter("MaxLength");

    public static readonly ValidationKeyDefinition MinLength =
        ValidationKeyDefinition.Create("Validation.MinLength")
            .WithStringParameter("FieldName")
            .WithIntParameter("MinLength");

    public static readonly ValidationKeyDefinition Predicate =
        ValidationKeyDefinition.Create("Validation.Predicate")
            .WithStringParameter("FieldName");
}
