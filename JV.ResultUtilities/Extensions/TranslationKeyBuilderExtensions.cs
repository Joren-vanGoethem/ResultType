using System.Collections.Generic;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.Extensions
{
    public static class TranslationKeyBuilderExtensions
    {
        public static ValidationKeyDefinition WithStringParameter(this ValidationKeyDefinition definition,
            string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.String, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithIntParameter(this ValidationKeyDefinition definition, string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Integer, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithDecimalParameter(this ValidationKeyDefinition definition,
            string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Decimal, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithDateTimeParameter(this ValidationKeyDefinition definition,
            string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.DateTime, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithTimeOnlyParameter(this ValidationKeyDefinition definition,
            string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.TimeOnly, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithDateOnlyParameter(this ValidationKeyDefinition definition,
            string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.DateOnly, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithBooleanParameter(this ValidationKeyDefinition definition,
            string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Boolean, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithGuidParameter(this ValidationKeyDefinition definition, string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Guid, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithEnumParameter(this ValidationKeyDefinition definition, string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Enum, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithUriParameter(this ValidationKeyDefinition definition, string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Uri, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithTimeSpanParameter(this ValidationKeyDefinition definition,
            string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.TimeSpan, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithEmailParameter(this ValidationKeyDefinition definition, string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Email, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithPhoneNumberParameter(this ValidationKeyDefinition definition,
            string name, object? defaultValue = null)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.PhoneNumber, defaultValue)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }
    }
}