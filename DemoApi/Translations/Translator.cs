using System.Globalization;
using JV.ResultUtilities.AspNetCore;
using JV.ResultUtilities.ValidationMessage;
using Microsoft.Extensions.Localization;
using Tenant.Api.Translations;

namespace DemoApi.Translations;

public interface ITranslator
{
    string Translate(string key);
    string Translate(string key, params object[] parameters);
}

// this is for dotnet to find the translation files names 'Translations'.
// if you want to name the files something else, rename this class
public class Translations { } 

public class Translator : ITranslator
{
    private readonly IStringLocalizer<Translations> _localizer;

    public Translator(IStringLocalizer<Translations> localizer)
    {
        _localizer = localizer;
    }

    public string Translate(string key)
    {
        return Translate(key, string.Empty);
    }

    public string Translate(string key, params object[] parameters)
    {
        var result = _localizer.GetString(key, parameters);

        if (result.ResourceNotFound)
        {
            var originalCulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = Culture.Default;
                var fallbackResult = _localizer.GetString(key, parameters);
                return fallbackResult.ResourceNotFound ? key : fallbackResult.Value;
            }
            finally
            {
                CultureInfo.CurrentUICulture = originalCulture;
            }
        }

        return result.Value;
    }
}

/// <summary>
/// Adapts the resx-backed <see cref="ITranslator"/> to the package's <see cref="IResultMessageTranslator"/>:
/// the translation key is the resx key, the formatted parameters fill its {0}… placeholders.
/// </summary>
public sealed class ResxResultMessageTranslator(ITranslator translator) : IResultMessageTranslator
{
    public string Translate(ValidationMessage message)
        => translator.Translate(message.TranslationKey, message.Parameters.ToArray<object>());
}
