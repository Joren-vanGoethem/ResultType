using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace JV.ResultUtilities.ValidationMessage
{
    public enum ParameterType
    {
        String,
        Integer,
        Decimal,
        DateTime,
        TimeOnly,
        DateOnly,
        Boolean,
        Guid,
        Enum,
        Uri,
        TimeSpan,
        Email,
        PhoneNumber
    }

    public class ValidationParameter
    {
        public string Name { get; }
        public ParameterType Type { get; }

        public ValidationParameter(string name, ParameterType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
        }

        public bool ValidateValue(object value)
        {
            if (value == null)
                return false;

            return Type switch
            {
                ParameterType.String => value is string,
                ParameterType.Integer => value is int || value is long || int.TryParse(value.ToString(), out _),
                ParameterType.Decimal => value is float || value is double || value is decimal ||
                                         decimal.TryParse(value.ToString(), out _),
                ParameterType.DateTime => value is DateTime || DateTime.TryParse(value.ToString(), out _),
                ParameterType.TimeOnly => value is TimeOnly || TimeOnly.TryParse(value.ToString(), out _),
                ParameterType.DateOnly => value is DateOnly || DateOnly.TryParse(value.ToString(), out _),
                ParameterType.Boolean => value is bool || bool.TryParse(value.ToString(), out _),
                ParameterType.Guid => value is Guid || Guid.TryParse(value.ToString(), out _),
                ParameterType.Enum => value is Enum,
                ParameterType.Uri => value is Uri || Uri.TryCreate(value.ToString(), UriKind.Absolute, out _),
                ParameterType.TimeSpan => value is TimeSpan || TimeSpan.TryParse(value.ToString(), out _),
                ParameterType.Email => value is string s && IsValidEmail(s),
                ParameterType.PhoneNumber => value is string p && IsValidPhoneNumber(p),
                _ => false
            };
        }

        public string FormatValue(object value)
        {
            if (!ValidateValue(value))
                throw new ArgumentException($"Value is not valid for parameter type {Type}", nameof(value));

            return value.ToString();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            // Basic validation - can be enhanced based on specific requirements
            return !string.IsNullOrWhiteSpace(phoneNumber) &&
                   phoneNumber.Length >= 8 &&
                   Regex.IsMatch(phoneNumber, @"^[0-9+\-\s()]*$");
        }
    }
}