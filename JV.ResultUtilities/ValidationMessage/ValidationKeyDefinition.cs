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

        /// <summary>
        /// Optional field name associated with this validation key, useful for mapping errors to form fields.
        /// </summary>
        public string? FieldName { get; }

        private ValidationKeyDefinition(string key, string translationKey,
            IEnumerable<ValidationParameter> parameters, string? fieldName = null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            TranslationKey = translationKey ?? throw new ArgumentNullException(nameof(translationKey));
            Parameters = parameters?.ToList().AsReadOnly() ?? new List<ValidationParameter>().AsReadOnly();
            FieldName = fieldName;
        }

        /// <summary>
        /// Sets the field name associated with this validation key.
        /// </summary>
        public ValidationKeyDefinition WithFieldName(string fieldName)
        {
            return new ValidationKeyDefinition(Key, TranslationKey, Parameters, fieldName);
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

        public bool ValidateParameters(object parameters)
        {
            if (parameters == null)
                return Parameters.All(p => p.DefaultValue != null && p.ValidateValue(p.DefaultValue));

            if (parameters is object[] positionalParams)
            {
                if (positionalParams.Length != Parameters.Count)
                    return false;

                for (int i = 0; i < Parameters.Count; i++)
                {
                    if (!Parameters[i].ValidateValue(positionalParams[i]))
                        return false;
                }

                return true;
            }

            if (parameters is string) // Strings are enumerable but we want to treat them as single values
            {
                // If it's a single value, it can only match if there is exactly one parameter or if there is exactly one parameter without a default value
                if (Parameters.Count(p => p.DefaultValue is null) != 1)
                    return false;
                
                return Parameters[0].ValidateValue(parameters);
            }

            if (parameters is IDictionary<string, object> dictionaryParams)
            {
                foreach (var parameter in Parameters)
                {
                    if (dictionaryParams.TryGetValue(parameter.Name, out var value))
                    {
                        if (!parameter.ValidateValue(value))
                            return false;
                    }
                    else if (parameter.DefaultValue == null)
                    {
                        return false;
                    }
                }

                return true;
            }

            // If it's an IEnumerable (and not string/object[] which are handled above), it might be positional
            if (parameters is System.Collections.IEnumerable enumerable)
            {
                var list = new List<object>();
                foreach (var item in enumerable) list.Add(item);

                if (list.Count == Parameters.Count)
                {
                    for (int i = 0; i < Parameters.Count; i++)
                    {
                        if (!Parameters[i].ValidateValue(list[i]))
                            return false;
                    }
                    return true;
                }
            }

            // Single-value check — must come before anonymous object check because primitive types
            // like decimal, Guid, DateTime etc. have reflection properties that would falsely match
            // the anonymous object branch
            if (Parameters.Count == 1 || Parameters.Count(p => p.DefaultValue == null) == 1)
            {
                var targetParam = Parameters.Count == 1
                    ? Parameters[0]
                    : Parameters.First(p => p.DefaultValue == null);

                if (targetParam.ValidateValue(parameters))
                    return true;
            }

            // Support anonymous objects
            var properties = parameters.GetType().GetProperties();
            if (properties.Length > 0 && properties.Any(p => p.CanRead && p.GetIndexParameters().Length == 0))
            {
                var propDict = properties
                    .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                    .ToDictionary(p => p.Name, p => p.GetValue(parameters));

                foreach (var parameter in Parameters)
                {
                    if (propDict.TryGetValue(parameter.Name, out var value))
                    {
                        if (!parameter.ValidateValue(value))
                            return false;
                    }
                    else if (parameter.DefaultValue == null)
                    {
                        return false;
                    }
                }

                return true;
            }

            // Final fallback: if there's only one parameter, maybe the 'parameters' object IS the value
            if (Parameters.Count == 1 || Parameters.Count(p => p.DefaultValue == null) == 1)
            {
                var targetParam = Parameters.Count == 1 
                    ? Parameters[0] 
                    : Parameters.First(p => p.DefaultValue == null);
                
                return targetParam.ValidateValue(parameters);
            }

            return false;
        }

        public string[] FormatParameters(object parameters)
        {
            if (!ValidateParameters(parameters))
                throw new ArgumentException("Parameters do not match the required definition", nameof(parameters));

            var result = new string[Parameters.Count];

            if (parameters == null)
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    result[i] = Parameters[i].FormatValue(Parameters[i].DefaultValue);
                }
            }
            else if (parameters is object[] positionalParams)
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    result[i] = Parameters[i].FormatValue(positionalParams[i] ?? Parameters[i].DefaultValue);
                }
            }
            else if (parameters is string)
            {
                if (Parameters.Count == 1)
                {
                    result[0] = Parameters[0].FormatValue(parameters);
                }
                else
                {
                    var targetIndex = -1;
                    for (int i = 0; i < Parameters.Count; i++)
                    {
                        if (Parameters[i].DefaultValue == null)
                        {
                            targetIndex = i;
                            break;
                        }
                    }

                    for (int i = 0; i < Parameters.Count; i++)
                    {
                        result[i] = Parameters[i].FormatValue(i == targetIndex ? parameters : Parameters[i].DefaultValue);
                    }
                }
            }
            else if (parameters is IDictionary<string, object> dictionaryParams)
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    var parameter = Parameters[i];
                    dictionaryParams.TryGetValue(parameter.Name, out var value);
                    result[i] = parameter.FormatValue(value ?? parameter.DefaultValue);
                }
            }
            else if (parameters is System.Collections.IEnumerable enumerable)
            {
                 var list = new List<object>();
                 foreach (var item in enumerable) list.Add(item);
                 if (list.Count == Parameters.Count)
                 {
                     for (int i = 0; i < Parameters.Count; i++)
                     {
                         result[i] = Parameters[i].FormatValue(list[i] ?? Parameters[i].DefaultValue);
                     }
                 }
                 else if (Parameters.Count == 1 || Parameters.Count(p => p.DefaultValue == null) == 1)
                 {
                     var targetIndex = Parameters.Count == 1 
                         ? 0 
                         : Enumerable.Range(0, Parameters.Count).First(i => Parameters[i].DefaultValue == null);

                     for (int i = 0; i < Parameters.Count; i++)
                     {
                         result[i] = Parameters[i].FormatValue(i == targetIndex ? parameters : Parameters[i].DefaultValue);
                     }
                 }
            }
            else
            {
                // Single-value check first — primitives like decimal/Guid have reflection properties
                // that would falsely match the anonymous object branch
                bool handledAsSingleValue = false;
                if (Parameters.Count == 1 || Parameters.Count(p => p.DefaultValue == null) == 1)
                {
                    var targetParam = Parameters.Count == 1
                        ? Parameters[0]
                        : Parameters.First(p => p.DefaultValue == null);

                    if (targetParam.ValidateValue(parameters))
                    {
                        var targetIndex = Parameters.Count == 1
                            ? 0
                            : Enumerable.Range(0, Parameters.Count).First(i => Parameters[i].DefaultValue == null);

                        for (int i = 0; i < Parameters.Count; i++)
                        {
                            result[i] = Parameters[i].FormatValue(i == targetIndex ? parameters : Parameters[i].DefaultValue);
                        }
                        handledAsSingleValue = true;
                    }
                }

                if (!handledAsSingleValue)
                {
                    var properties = parameters.GetType().GetProperties();
                    var filteredProps = properties.Where(p => p.CanRead && p.GetIndexParameters().Length == 0).ToList();

                    if (filteredProps.Count > 0)
                    {
                        var propDict = filteredProps.ToDictionary(p => p.Name, p => p.GetValue(parameters));
                        for (int i = 0; i < Parameters.Count; i++)
                        {
                            var parameter = Parameters[i];
                            propDict.TryGetValue(parameter.Name, out var value);
                            result[i] = parameter.FormatValue(value ?? parameter.DefaultValue);
                        }
                    }
                }
            }

            return result;
        }
    }
}