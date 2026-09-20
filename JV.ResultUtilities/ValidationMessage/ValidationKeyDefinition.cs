using System;
using System.Collections.Generic;
using System.Linq;

namespace JV.ResultUtilities.ValidationMessage
{
    public class ValidationKeyDefinition
    {
        private static readonly IReadOnlyDictionary<string, object> EmptyMetadata =
            new Dictionary<string, object>();

        public string Key { get; }
        public string TranslationKey { get; }
        public IReadOnlyList<ValidationParameter> Parameters { get; }

        /// <summary>
        /// Optional field name associated with this validation key, useful for mapping errors to form fields.
        /// </summary>
        public string? FieldName { get; }

        /// <summary>
        /// Arbitrary, immutable metadata attached to this key by the code that declares it — for example the
        /// HTTP status an API layer should answer with, or a severity. The core library never reads it;
        /// it exists so a key can carry meaning that only a consuming layer interprets, without that layer
        /// having to keep a second registry keyed on the key string.
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata { get; }

        private ValidationKeyDefinition(string key, string translationKey,
            IEnumerable<ValidationParameter> parameters, string? fieldName = null,
            IReadOnlyDictionary<string, object>? metadata = null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            TranslationKey = translationKey ?? throw new ArgumentNullException(nameof(translationKey));
            Parameters = parameters?.ToList().AsReadOnly() ?? new List<ValidationParameter>().AsReadOnly();
            FieldName = fieldName;
            Metadata = metadata ?? EmptyMetadata;
        }

        /// <summary>
        /// Sets the field name associated with this validation key.
        /// </summary>
        public ValidationKeyDefinition WithFieldName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or whitespace.", nameof(fieldName));

            return new ValidationKeyDefinition(Key, TranslationKey, Parameters, fieldName, Metadata);
        }

        /// <summary>
        /// Returns a copy of this key with <paramref name="value"/> stored under <paramref name="name"/> in
        /// <see cref="Metadata"/>. An existing entry with the same name is replaced.
        /// </summary>
        public ValidationKeyDefinition WithMetadata(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var metadata = new Dictionary<string, object>(Metadata) { [name] = value };
            return new ValidationKeyDefinition(Key, TranslationKey, Parameters, FieldName, metadata);
        }

        /// <summary>
        /// Reads a metadata entry as <typeparamref name="T"/>. Returns false when the entry is absent or
        /// holds a value of another type.
        /// </summary>
        public bool TryGetMetadata<T>(string name, out T value)
        {
            if (Metadata.TryGetValue(name, out var stored) && stored is T typed)
            {
                value = typed;
                return true;
            }

            value = default!;
            return false;
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
            var values = ResolveParameterValues(parameters);
            var result = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Parameters[i].FormatValue(values[i]!);
            }

            return result;
        }

        /// <summary>
        /// Resolves the caller-supplied <paramref name="parameters"/> — positional array, single value,
        /// dictionary, anonymous object or other enumerable — into one raw value per declared
        /// <see cref="Parameters"/> entry, in declaration order, with defaults applied. This is the untyped
        /// counterpart of <see cref="FormatParameters"/>; the two always agree on which value fills which slot.
        /// </summary>
        public object?[] ResolveParameterValues(object? parameters)
        {
            if (!ValidateParameters(parameters!))
                throw new ArgumentException("Parameters do not match the required definition", nameof(parameters));

            var result = new object?[Parameters.Count];

            if (parameters == null)
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    result[i] = Parameters[i].DefaultValue;
                }
            }
            else if (parameters is object[] positionalParams)
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    result[i] = positionalParams[i] ?? Parameters[i].DefaultValue;
                }
            }
            else if (parameters is string)
            {
                FillSingleValue(parameters, result);
            }
            else if (parameters is IDictionary<string, object> dictionaryParams)
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    var parameter = Parameters[i];
                    dictionaryParams.TryGetValue(parameter.Name, out var value);
                    result[i] = value ?? parameter.DefaultValue;
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
                        result[i] = list[i] ?? Parameters[i].DefaultValue;
                    }
                }
                else if (Parameters.Count == 1 || Parameters.Count(p => p.DefaultValue == null) == 1)
                {
                    FillSingleValue(parameters, result);
                }
                else
                {
                    // IEnumerable count didn't match and not a single-value scenario.
                    // Fall through to anonymous object handling (the object may have named properties).
                    ResolveFromReflection(parameters, result);
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
                        FillSingleValue(parameters, result);
                        handledAsSingleValue = true;
                    }
                }

                if (!handledAsSingleValue)
                {
                    ResolveFromReflection(parameters, result);
                }
            }

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] == null)
                {
                    throw new InvalidOperationException(
                        $"Parameter '{Parameters[i].Name}' at index {i} was not resolved. " +
                        $"This indicates a mismatch between ValidateParameters and ResolveParameterValues for input type '{parameters?.GetType().FullName}'.");
                }
            }

            return result;
        }

        /// <summary>
        /// Places a single supplied value into the one slot that has no default, and defaults everywhere else.
        /// </summary>
        private void FillSingleValue(object value, object?[] result)
        {
            var targetIndex = Parameters.Count == 1
                ? 0
                : Enumerable.Range(0, Parameters.Count).First(i => Parameters[i].DefaultValue == null);

            for (int i = 0; i < Parameters.Count; i++)
            {
                result[i] = i == targetIndex ? value : Parameters[i].DefaultValue;
            }
        }

        private void ResolveFromReflection(object parameters, object?[] result)
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
                    result[i] = value ?? parameter.DefaultValue;
                }
            }
        }
    }
}