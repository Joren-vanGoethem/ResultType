using System;
using System.Threading.Tasks;
using JV.ResultUtilities.ValidationMessage;
using JV.ResultUtilities.ValidationPipeline;

namespace JV.ResultUtilities.FluentValidation;

public class RuleBuilder<T, TProperty>
{
    private readonly ValidationPipeline<T> _pipeline;
    private readonly Func<T, TProperty> _accessor;
    private RuleConfig? _lastRuleConfig;

    internal string PropertyName { get; }

    internal RuleBuilder(ValidationPipeline<T> pipeline, Func<T, TProperty> accessor, string propertyName)
    {
        _pipeline = pipeline;
        _accessor = accessor;
        PropertyName = propertyName;
    }

    /// <summary>
    /// Adds a synchronous validation rule to the pipeline.
    /// </summary>
    internal RuleBuilder<T, TProperty> AddSyncRule(
        Func<TProperty, bool> predicate,
        ValidationKeyDefinition defaultKey,
        Func<T, object?>? defaultParametersFactory)
    {
        var config = new RuleConfig(defaultKey.WithFieldName(PropertyName), defaultParametersFactory);
        _lastRuleConfig = config;

        _pipeline.AddRule(instance =>
        {
            var value = _accessor(instance);
            if (predicate(value))
                return Result.Ok();

            var parameters = config.ParametersFactory?.Invoke(instance);
            return parameters is not null
                ? Result.Error(config.Key, parameters)
                : Result.Error(config.Key);
        });

        return this;
    }

    /// <summary>
    /// Adds an asynchronous validation rule to the pipeline.
    /// </summary>
    internal RuleBuilder<T, TProperty> AddAsyncRule(
        Func<TProperty, Task<bool>> predicate,
        ValidationKeyDefinition defaultKey,
        Func<T, object?>? defaultParametersFactory)
    {
        var config = new RuleConfig(defaultKey.WithFieldName(PropertyName), defaultParametersFactory);
        _lastRuleConfig = config;

        _pipeline.AddRule(async instance =>
        {
            var value = _accessor(instance);
            if (await predicate(value))
                return Result.Ok();

            var parameters = config.ParametersFactory?.Invoke(instance);
            return parameters is not null
                ? Result.Error(config.Key, parameters)
                : Result.Error(config.Key);
        });

        return this;
    }

    /// <summary>
    /// Validates using a custom synchronous predicate. Uses default "Validation.Predicate" key.
    /// </summary>
    public RuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate)
    {
        return AddSyncRule(predicate, BuiltInValidationKeys.Predicate, _ => PropertyName);
    }

    /// <summary>
    /// Validates using a custom asynchronous predicate. Uses default "Validation.Predicate" key.
    /// </summary>
    public RuleBuilder<T, TProperty> MustAsync(Func<TProperty, Task<bool>> predicate)
    {
        return AddAsyncRule(predicate, BuiltInValidationKeys.Predicate, _ => PropertyName);
    }

    /// <summary>
    /// Overrides the ValidationKeyDefinition for the most recently added rule.
    /// Use this for parameterless custom keys.
    /// </summary>
    public RuleBuilder<T, TProperty> WithMessage(ValidationKeyDefinition key)
    {
        if (_lastRuleConfig is null)
            throw new InvalidOperationException("WithMessage must be called after a rule method.");

        _lastRuleConfig.Key = key.WithFieldName(PropertyName);
        _lastRuleConfig.ParametersFactory = null;
        return this;
    }

    /// <summary>
    /// Overrides the ValidationKeyDefinition and parameters for the most recently added rule.
    /// The parametersFactory receives the validated instance and should return an object
    /// matching the custom key's parameter definition (anonymous object, object[], or single value).
    /// </summary>
    public RuleBuilder<T, TProperty> WithMessage(ValidationKeyDefinition key, Func<T, object> parametersFactory)
    {
        if (_lastRuleConfig is null)
            throw new InvalidOperationException("WithMessage must be called after a rule method.");

        _lastRuleConfig.Key = key.WithFieldName(PropertyName);
        _lastRuleConfig.ParametersFactory = parametersFactory;
        return this;
    }

    private sealed class RuleConfig(ValidationKeyDefinition key, Func<T, object?>? parametersFactory)
    {
        public ValidationKeyDefinition Key { get; set; } = key;
        public Func<T, object?>? ParametersFactory { get; set; } = parametersFactory;
    }
}
