using System;
using System.Linq;

namespace JV.ResultUtilities.ValidationMessage
{
    public class ValidationMessage
    {
        public string TranslationKey { get; }
        public string[] Parameters { get; }
        public ValidationKeyDefinition KeyDefinition { get; }

        protected ValidationMessage(ValidationKeyDefinition keyDefinition, object parameters)
        {
            if (keyDefinition == null) throw new ArgumentNullException(nameof(keyDefinition));
            if (!keyDefinition.ValidateParameters(parameters))
            {
                int receivedCount = 0;
                if (parameters is object[] pArray) receivedCount = pArray.Length;
                else if (parameters is System.Collections.ICollection col) receivedCount = col.Count;
                else if (parameters != null) receivedCount = parameters.GetType().GetProperties().Length;

                throw new ArgumentException(
                    $"Parameters do not match the required definition. Expected {keyDefinition.Parameters.Count} parameters of types: {string.Join(", ", keyDefinition.Parameters.Select(p => $"{p.Name} ({p.Type})"))}. Received {receivedCount} parameters.",
                    nameof(parameters));
            }

            KeyDefinition = keyDefinition;
            TranslationKey = keyDefinition.TranslationKey;
            Parameters = keyDefinition.FormatParameters(parameters);
        }

        public static ValidationMessage Create(ValidationKeyDefinition keyDefinition, object parameters)
        {
            return new ValidationMessage(keyDefinition, parameters);
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