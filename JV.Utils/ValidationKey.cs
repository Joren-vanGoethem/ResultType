using System;

namespace JV.Utils
{
    [Obsolete("Use TranslationKeyDefinition instead")]
    public class ValidationKey
    {
        public string Key { get; }
        public string TranslationKey { get; }

        private ValidationKey(string key, string translationKey)
        {
            Key = key;
            TranslationKey = translationKey;
        }

        public static ValidationKey Create(string key, string translationKey)
        {
            return new ValidationKey(key, translationKey);
        }
    }
}