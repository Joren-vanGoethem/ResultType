namespace JV.ResultUtilities.FluentValidation;

public static class CommonRuleExtensions
{
    /// <summary>
    /// Validates that the property value is not null.
    /// </summary>
    public static RuleBuilder<T, TProperty> NotNull<T, TProperty>(this RuleBuilder<T, TProperty> builder)
    {
        return builder.AddSyncRule(
            value => value is not null,
            BuiltInValidationKeys.NotNull,
            _ => builder.PropertyName);
    }
}
