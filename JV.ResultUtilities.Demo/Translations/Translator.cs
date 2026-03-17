using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace JV.ResultUtilities.Demo.Translations;

public class Translations;

public interface ITranslator
{
    string Translate(string key);
    string Translate(string key, params object[] parameters);
    string TranslateValidationMessage(JV.ResultUtilities.ValidationMessage.ValidationMessage message);
    ValidationProblemDetails ToValidationProblemDetails(IEnumerable<JV.ResultUtilities.ValidationMessage.ValidationMessage> messages);
}

public class Translator : ITranslator
{
    private readonly IStringLocalizer<Translations> _localizer;

    public Translator(IStringLocalizer<Translations> localizer)
    {
        _localizer = localizer;
    }

    public string Translate(string key)
    {
        return Translate(key, []);
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

    public string TranslateValidationMessage(JV.ResultUtilities.ValidationMessage.ValidationMessage message)
    {
        var parameters = message.RawParameters?.Cast<object>().ToArray()
                         ?? message.Parameters.Cast<object>().ToArray();

        return Translate(message.TranslationKey, parameters);
    }

    public ValidationProblemDetails ToValidationProblemDetails(IEnumerable<JV.ResultUtilities.ValidationMessage.ValidationMessage> messages)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var message in messages)
        {
            var field = message.KeyDefinition?.FieldName
                        ?? message.KeyDefinition?.Key
                        ?? message.TranslationKey;

            var localizedText = TranslateValidationMessage(message);

            if (!errors.TryGetValue(field, out var existing))
                errors[field] = [localizedText];
            else
                errors[field] = [..existing, localizedText];
        }

        return new ValidationProblemDetails(errors)
        {
            Status = 422,
            Title = "Validation Failed",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.21"
        };
    }
}
