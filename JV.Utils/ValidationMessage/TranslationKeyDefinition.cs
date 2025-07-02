using System;
using System.Collections.Generic;
using System.Linq;

namespace JV.Utils
{
    public class TranslationKeyDefinition
    {
        public string Key { get; }
        public string TranslationKey { get; }
        public IReadOnlyList<TranslationParameter> Parameters { get; }

        private TranslationKeyDefinition(string key, string translationKey,
            IEnumerable<TranslationParameter> parameters)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            TranslationKey = translationKey ?? throw new ArgumentNullException(nameof(translationKey));
            Parameters = parameters?.ToList().AsReadOnly() ?? new List<TranslationParameter>().AsReadOnly();
        }

        public static TranslationKeyDefinition Create(string key, string translationKey)
        {
            return new TranslationKeyDefinition(key, translationKey, new List<TranslationParameter>());
        }

        public static TranslationKeyDefinition Create(string key) // usefull when key is also the translationKey
        {
            return new TranslationKeyDefinition(key, key, new List<TranslationParameter>());
        }

        public static TranslationKeyDefinition Create(string key, string translationKey,
            params TranslationParameter[] parameters)
        {
            return new TranslationKeyDefinition(key, translationKey, parameters);
        }

        public static TranslationKeyDefinition Create(string key, params TranslationParameter[] parameters)
        {
            return new TranslationKeyDefinition(key, key, parameters);
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