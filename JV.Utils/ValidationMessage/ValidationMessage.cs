using System;
using System.Linq;

namespace JV.Utils
{
  public class ValidationMessage
  {
    public string TranslationKey { get; }
    public string[] Parameters { get; }
    public TranslationKeyDefinition KeyDefinition { get; }

    protected ValidationMessage(TranslationKeyDefinition keyDefinition, object[] parameters)
    {
      if (keyDefinition == null) throw new ArgumentNullException(nameof(keyDefinition));
      if (!keyDefinition.ValidateParameters(parameters))
        throw new ArgumentException("Parameters do not match the required definition", nameof(parameters));

      KeyDefinition = keyDefinition;
      TranslationKey = keyDefinition.TranslationKey;
      Parameters = keyDefinition.FormatParameters(parameters);
    }

    public static ValidationMessage CreateError(TranslationKeyDefinition keyDefinition, params object[] parameters)
    {
      return new ValidationMessage(keyDefinition, parameters);
    }

    public string MapToErrorMessage()
    {
      if (Parameters.Any())
        return
          $"ValidationKey: {TranslationKey} Parameters: {string.Join(", ", Parameters).Replace("{", "{{").Replace("}", "}}")}";

      return $"ValidationKey: {TranslationKey}";
    }
  }
}