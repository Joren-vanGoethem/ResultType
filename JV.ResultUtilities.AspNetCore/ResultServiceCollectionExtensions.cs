using System.Reflection;
using JV.ResultUtilities.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JV.ResultUtilities.AspNetCore;

public static class ResultServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ResultExceptionHandler"/> (with <c>AddProblemDetails()</c>) and the
    /// <see cref="KeyResultMessageTranslator"/> fallback. Pair it with <c>app.UseExceptionHandler()</c>.
    /// Register your own <see cref="IResultMessageTranslator"/> with the generic overload, or before
    /// this call — the fallback is added with <c>TryAdd</c> and never replaces yours.
    /// </summary>
    public static IServiceCollection AddResultProblemDetails(
        this IServiceCollection services,
        Action<ResultProblemDetailsOptions>? configure = null)
    {
        services.AddProblemDetails();
        services.AddOptions<ResultProblemDetailsOptions>();
        if (configure != null) services.Configure(configure);
        services.TryAddSingleton<IResultMessageTranslator, KeyResultMessageTranslator>();
        services.AddExceptionHandler<ResultExceptionHandler>();
        return services;
    }

    /// <summary>Same as <see cref="AddResultProblemDetails(IServiceCollection, Action{ResultProblemDetailsOptions})"/>, with <typeparamref name="TTranslator"/> as the translator.</summary>
    public static IServiceCollection AddResultProblemDetails<TTranslator>(
        this IServiceCollection services,
        Action<ResultProblemDetailsOptions>? configure = null)
        where TTranslator : class, IResultMessageTranslator
    {
        services.Replace(ServiceDescriptor.Singleton<IResultMessageTranslator, TTranslator>());
        return services.AddResultProblemDetails(configure);
    }

    /// <summary>
    /// Registers every concrete <see cref="IValidator"/> in <paramref name="assemblies"/> (the calling
    /// assembly when none are given) and installs <see cref="ValidateModelFilter"/> as a global MVC
    /// filter, once. Validators are singletons: an <c>AbstractValidator&lt;T&gt;</c> is a rule set, not
    /// request state.
    /// </summary>
    public static IServiceCollection AddResultValidation(this IServiceCollection services, params Assembly[] assemblies)
    {
        var scan = assemblies.Length == 0 ? [Assembly.GetCallingAssembly()] : assemblies;

        foreach (var type in scan.SelectMany(SafeTypes))
        {
            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition) continue;
            if (!typeof(IValidator).IsAssignableFrom(type)) continue;

            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IValidator), type));
        }

        return services.AddValidateModelFilter();
    }

    /// <summary>Registers one validator and installs the filter, for callers that prefer no scanning.</summary>
    public static IServiceCollection AddResultValidator<TValidator>(this IServiceCollection services)
        where TValidator : class, IValidator
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidator, TValidator>());
        return services.AddValidateModelFilter();
    }

    private static IServiceCollection AddValidateModelFilter(this IServiceCollection services)
    {
        services.TryAddSingleton<ValidateModelFilter>();
        services.Configure<MvcOptions>(options =>
        {
            var installed = options.Filters
                .OfType<ServiceFilterAttribute>()
                .Any(f => f.ServiceType == typeof(ValidateModelFilter));
            if (!installed)
                options.Filters.AddService<ValidateModelFilter>();
        });
        return services;
    }

    private static IEnumerable<Type> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null)!;
        }
    }
}
