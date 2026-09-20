using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.AspNetCore;

// Inside JV.ResultUtilities.* the bare name resolves to the ValidationMessage *namespace*; an alias
// declared inside this namespace takes precedence and restores the type.
using ValidationMessage = JV.ResultUtilities.ValidationMessage.ValidationMessage;

/// <summary>
/// Turns a <see cref="ValidationMessage"/> into the human-readable text the client receives.
/// Register your own (resx, database, whatever) with
/// <see cref="ResultServiceCollectionExtensions.AddResultProblemDetails{TTranslator}"/>; without one the
/// handler falls back to <see cref="KeyResultMessageTranslator"/>.
/// </summary>
public interface IResultMessageTranslator
{
    string Translate(ValidationMessage message);
}

/// <summary>
/// Default translator: returns the translation key itself. Never produces an empty string, so a
/// missing translation degrades to a stable identifier a developer can search for.
/// </summary>
public sealed class KeyResultMessageTranslator : IResultMessageTranslator
{
    public string Translate(ValidationMessage message) => message.TranslationKey;
}
