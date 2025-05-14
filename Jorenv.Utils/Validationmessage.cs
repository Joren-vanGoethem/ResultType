using System.Linq;

namespace Jorenv.Utils
{
    public class ValidationMessage
    {
        public SeverityLevel SeverityLevel { get; }
        public string TranslationKey { get; }
        public string[] Parameters { get; }

        protected ValidationMessage(SeverityLevel severityLevel, string translationKey, string[] parameters)
        {
            SeverityLevel = severityLevel;
            TranslationKey = translationKey;
            Parameters = parameters;
        }

        public static ValidationMessage CreateError(string translationKey, string[] parameters)
        {
            return new ValidationMessage(SeverityLevel.Error, translationKey, parameters);
        }

        public static ValidationMessage CreateError(string translationKey, params object[] parameters)
        {
            return new ValidationMessage(SeverityLevel.Error, translationKey,
                parameters.Select(p => p.ToString()).ToArray());
        }

        public static ValidationMessage CreateWarning(string translationKey, string[] parameters)
        {
            return new ValidationMessage(SeverityLevel.Warning, translationKey, parameters);
        }
        
        public static ValidationMessage CreateWarning(string translationKey, params object[] parameters)
        {
            return new ValidationMessage(SeverityLevel.Warning, translationKey,
                parameters.Select(p => p.ToString()).ToArray());
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