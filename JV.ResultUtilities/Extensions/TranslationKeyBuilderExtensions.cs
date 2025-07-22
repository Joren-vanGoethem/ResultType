using System.Collections.Generic;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.Extensions
{
    public static class TranslationKeyBuilderExtensions
    {
        public static ValidationKeyDefinition WithStringParameter(this ValidationKeyDefinition definition,
            string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.String)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithIntParameter(this ValidationKeyDefinition definition, string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Integer)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithDecimalParameter(this ValidationKeyDefinition definition,
            string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Decimal)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithDateTimeParameter(this ValidationKeyDefinition definition,
            string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.DateTime)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithTimeOnlyParameter(this ValidationKeyDefinition definition,
            string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.TimeOnly)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithDateOnlyParameter(this ValidationKeyDefinition definition,
            string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.DateOnly)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithBooleanParameter(this ValidationKeyDefinition definition,
            string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Boolean)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithGuidParameter(this ValidationKeyDefinition definition, string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Guid)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithEnumParameter(this ValidationKeyDefinition definition, string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Enum)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithUriParameter(this ValidationKeyDefinition definition, string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Uri)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithTimeSpanParameter(this ValidationKeyDefinition definition,
            string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.TimeSpan)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithEmailParameter(this ValidationKeyDefinition definition, string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.Email)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static ValidationKeyDefinition WithPhoneNumberParameter(this ValidationKeyDefinition definition,
            string name)
        {
            var parameters = new List<ValidationParameter>(definition.Parameters)
            {
                new ValidationParameter(name, ParameterType.PhoneNumber)
            };

            return ValidationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }
    }
}