using System;
using System.Linq;

namespace JV.ResultUtilities.ValidationMessage
{
  public class ValidationMessage
  {
    public string TranslationKey { get; }
    public string[] Parameters { get; }
    public ValidationKeyDefinition KeyDefinition { get; }

    protected ValidationMessage(ValidationKeyDefinition keyDefinition, object[] parameters)
    {
      if (keyDefinition == null) throw new ArgumentNullException(nameof(keyDefinition));
      if (!keyDefinition.ValidateParameters(parameters))
        throw new ArgumentException(
          $"Parameters do not match the required definition. Expected {keyDefinition.Parameters.Count} parameters of types: {string.Join(", ", keyDefinition.Parameters.Select(p => $"{p.Name} ({p.Type})"))}. Received {(parameters?.Length ?? 0)} parameters.",
          nameof(parameters));

      KeyDefinition = keyDefinition;
      TranslationKey = keyDefinition.TranslationKey;
      Parameters = keyDefinition.FormatParameters(parameters);
    }

    public static ValidationMessage Create(ValidationKeyDefinition keyDefinition, params object[] parameters)
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