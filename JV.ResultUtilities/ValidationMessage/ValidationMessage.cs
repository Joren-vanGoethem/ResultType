using System;
using System.Collections.Generic;
using System.Linq;

namespace JV.ResultUtilities.ValidationMessage
{
    public class ValidationMessage
    {
        public string TranslationKey { get; }
        public string[] Parameters { get; }
        public ValidationKeyDefinition KeyDefinition { get; }

        /// <summary>
        /// The original typed parameter values before string formatting.
        /// Null when parameters were not preserved (e.g., lenient creation with no params).
        /// </summary>
        public object[]? RawParameters { get; }

        /// <summary>
        /// The raw parameter values keyed by the parameter names declared on <see cref="KeyDefinition"/>,
        /// with defaults applied — the same values <see cref="Parameters"/> holds, before formatting and
        /// with their names attached. This is what a structured error body should serialise: a client
        /// can read <c>parameters.maximum</c> instead of guessing which <c>{n}</c> placeholder it was.
        /// For a lenient message, names are matched by position and any surplus value is keyed by its index.
        /// </summary>
        public IReadOnlyDictionary<string, object?> NamedParameters { get; }

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
            var resolved = keyDefinition.ResolveParameterValues(parameters);
            Parameters = new string[resolved.Length];
            for (var i = 0; i < resolved.Length; i++)
                Parameters[i] = keyDefinition.Parameters[i].FormatValue(resolved[i]!);

            RawParameters = parameters is object[] arr ? arr : parameters != null ? [parameters] : null;
            NamedParameters = ZipNames(keyDefinition, resolved);
        }

        private readonly record struct LenientMarker;

        private ValidationMessage(ValidationKeyDefinition keyDefinition, object[]? rawParameters, LenientMarker _)
        {
            if (keyDefinition == null) throw new ArgumentNullException(nameof(keyDefinition));

            KeyDefinition = keyDefinition;
            TranslationKey = keyDefinition.TranslationKey;
            RawParameters = rawParameters;

            if (rawParameters != null)
                Parameters = rawParameters.Select(p => p?.ToString() ?? string.Empty).ToArray();
            else
                Parameters = [];

            NamedParameters = ZipNames(keyDefinition, rawParameters ?? []);
        }

        private static IReadOnlyDictionary<string, object?> ZipNames(ValidationKeyDefinition keyDefinition, object?[] values)
        {
            var named = new Dictionary<string, object?>(values.Length);
            for (var i = 0; i < values.Length; i++)
            {
                var name = i < keyDefinition.Parameters.Count
                    ? keyDefinition.Parameters[i].Name
                    : i.ToString();
                named[name] = values[i];
            }

            return named;
        }

        public static ValidationMessage Create(ValidationKeyDefinition keyDefinition, object parameters)
        {
            return new ValidationMessage(keyDefinition, parameters);
        }

        public static ValidationMessage Create(ValidationKeyDefinition keyDefinition, params object[] parameters)
        {
            return new ValidationMessage(keyDefinition, parameters);
        }

        /// <summary>
        /// Creates a ValidationMessage without parameter type validation.
        /// Parameters are converted to strings via ToString().
        /// </summary>
        public static ValidationMessage CreateLenient(ValidationKeyDefinition keyDefinition, params object[] parameters)
        {
            return new ValidationMessage(keyDefinition, parameters, default(LenientMarker));
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
