using System.Collections.Generic;

namespace JV.Utils.Extensions
{
    public static class TranslationKeyBuilderExtensions
    {
        public static TranslationKeyDefinition WithStringParameter(this TranslationKeyDefinition definition,
            string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.String)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithIntParameter(this TranslationKeyDefinition definition, string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.Integer)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithDecimalParameter(this TranslationKeyDefinition definition,
            string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.Decimal)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithDateTimeParameter(this TranslationKeyDefinition definition,
            string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.DateTime)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithTimeOnlyParameter(this TranslationKeyDefinition definition,
            string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.TimeOnly)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithDateOnlyParameter(this TranslationKeyDefinition definition,
            string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.DateOnly)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithBooleanParameter(this TranslationKeyDefinition definition,
            string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.Boolean)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithGuidParameter(this TranslationKeyDefinition definition, string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.Guid)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithEnumParameter(this TranslationKeyDefinition definition, string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.Enum)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithUriParameter(this TranslationKeyDefinition definition, string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.Uri)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithTimeSpanParameter(this TranslationKeyDefinition definition,
            string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.TimeSpan)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithEmailParameter(this TranslationKeyDefinition definition, string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.Email)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }

        public static TranslationKeyDefinition WithPhoneNumberParameter(this TranslationKeyDefinition definition,
            string name)
        {
            var parameters = new List<TranslationParameter>(definition.Parameters)
            {
                new TranslationParameter(name, ParameterType.PhoneNumber)
            };

            return TranslationKeyDefinition.Create(definition.Key, definition.TranslationKey, parameters.ToArray());
        }
    }
}