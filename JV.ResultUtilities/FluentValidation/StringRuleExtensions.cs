namespace JV.ResultUtilities.FluentValidation;

public static class StringRuleExtensions
{
    /// <summary>
    /// Validates that the string is not null, empty, or whitespace.
    /// </summary>
    public static RuleBuilder<T, string> NotEmpty<T>(this RuleBuilder<T, string> builder)
    {
        return builder.AddSyncRule(
            value => !string.IsNullOrWhiteSpace(value),
            BuiltInValidationKeys.NotEmpty,
            _ => builder.PropertyName);
    }

    /// <summary>
    /// Validates that the string length does not exceed the specified maximum.
    /// Null strings pass (use NotEmpty or NotNull to reject nulls).
    /// </summary>
    public static RuleBuilder<T, string> MaxLength<T>(this RuleBuilder<T, string> builder, int max)
    {
        return builder.AddSyncRule(
            value => value is null || value.Length <= max,
            BuiltInValidationKeys.MaxLength,
            _ => new object[] { builder.PropertyName, max });
    }

    /// <summary>
    /// Validates that the string length meets the specified minimum.
    /// Null strings pass (use NotEmpty or NotNull to reject nulls).
    /// </summary>
    public static RuleBuilder<T, string> MinLength<T>(this RuleBuilder<T, string> builder, int min)
    {
        return builder.AddSyncRule(
            value => value is null || value.Length >= min,
            BuiltInValidationKeys.MinLength,
            _ => new object[] { builder.PropertyName, min });
    }
}
