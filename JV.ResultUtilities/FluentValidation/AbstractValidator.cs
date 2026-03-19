using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JV.ResultUtilities.ValidationPipeline;

namespace JV.ResultUtilities.FluentValidation;

public abstract class AbstractValidator<T> : IValidator
{
    private readonly ValidationPipeline<T> _pipeline = new();

    /// <summary>
    /// When true, validation stops on the first failure instead of collecting all errors.
    /// Delegates to the underlying ValidationPipeline.
    /// </summary>
    public bool ShortCircuit
    {
        get => _pipeline.ShortCircuit;
        set => _pipeline.ShortCircuit = value;
    }

    /// <summary>
    /// Begins a rule chain for the specified property.
    /// The property name is automatically extracted from the expression for FieldName binding.
    /// </summary>
    protected RuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var propertyName = ExtractPropertyName(expression);
        var accessor = expression.Compile();
        return new RuleBuilder<T, TProperty>(_pipeline, accessor, propertyName);
    }

    public Result<T> Validate(T instance) => _pipeline.Validate(instance);

    public Task<Result<T>> ValidateAsync(T instance) => _pipeline.ValidateAsync(instance);

    Type IValidator.ValidatedType => typeof(T);

    ResultType IValidator.ValidateObject(object instance) => Validate((T)instance);

    async Task<ResultType> IValidator.ValidateObjectAsync(object instance) => await ValidateAsync((T)instance);

    private static string ExtractPropertyName<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var body = expression.Body;

        // Unwrap Convert/ConvertChecked (common with value types)
        if (body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary)
            body = unary.Operand;

        if (body is MemberExpression member)
            return member.Member.Name;

        throw new ArgumentException(
            $"Expression '{expression}' does not refer to a property or field. " +
            "RuleFor requires a simple member access expression like x => x.PropertyName.",
            nameof(expression));
    }
}
