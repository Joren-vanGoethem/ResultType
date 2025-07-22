using System;
using System.Collections.Generic;
using System.Linq;

namespace JV.ResultUtilities.ValidationMessage
{
    public class ValidationKeyDefinition
    {
        public string Key { get; }
        public string TranslationKey { get; }
        public IReadOnlyList<ValidationParameter> Parameters { get; }

        private ValidationKeyDefinition(string key, string translationKey,
            IEnumerable<ValidationParameter> parameters)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            TranslationKey = translationKey ?? throw new ArgumentNullException(nameof(translationKey));
            Parameters = parameters?.ToList().AsReadOnly() ?? new List<ValidationParameter>().AsReadOnly();
        }

        public static ValidationKeyDefinition Create(string key, string translationKey)
        {
            return new ValidationKeyDefinition(key, translationKey, new List<ValidationParameter>());
        }

        public static ValidationKeyDefinition Create(string key) // usefull when key is also the translationKey
        {
            return new ValidationKeyDefinition(key, key, new List<ValidationParameter>());
        }

        public static ValidationKeyDefinition Create(string key, string translationKey,
            params ValidationParameter[] parameters)
        {
            return new ValidationKeyDefinition(key, translationKey, parameters);
        }

        public static ValidationKeyDefinition Create(string key, params ValidationParameter[] parameters)
        {
            return new ValidationKeyDefinition(key, key, parameters);
        }

        public bool ValidateParameters(object[] parameters)
        {
            if (parameters == null && Parameters.Count > 0)
                return false;

            if (parameters?.Length != Parameters.Count)
                return false;

            for (int i = 0; i < Parameters.Count; i++)
            {
                if (!Parameters[i].ValidateValue(parameters[i]))
                    return false;
            }

            return true;
        }

        public string[] FormatParameters(object[] parameters)
        {
            if (!ValidateParameters(parameters))
                throw new ArgumentException("Parameters do not match the required definition", nameof(parameters));

            var result = new string[Parameters.Count];
            for (int i = 0; i < Parameters.Count; i++)
            {
                result[i] = Parameters[i].FormatValue(parameters[i]);
            }

            return result;
        }
    }
}