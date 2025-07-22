using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JV.ResultUtilities.ValidationMessage;
using JV.ResultUtilities.Extensions;

namespace JV.ResultUtilities
{
    public class Result<TValue> : ResultType
    {
        
        private static readonly ValidationKeyDefinition UsernameInvalidKey = ValidationKeyDefinition
            .Create("user.username.invalid")
            .WithStringParameter("username")
            .WithIntParameter("minLength");

        
        public TValue Value { get; }

        private Result(TValue value, IEnumerable<ValidationMessage.ValidationMessage> validationMessages)
        {
            Value = value;
            ValidationMessages = validationMessages;
        }

        public static Result<TValue> Create(TValue value)
            => new Result<TValue>(value, []);

        public static Result<TValue> Create(TValue value, IEnumerable<ValidationMessage.ValidationMessage> validationMessages)
            => new Result<TValue>(value, validationMessages);

        public Result<TValue> Merge(params Result[] results)
        {
            return new Result<TValue>(Value, ValidationMessages.Concat(results.SelectMany(r => r.ValidationMessages)));
        }

        public void Deconstruct(out bool isSuccessful, out IEnumerable<ValidationMessage.ValidationMessage> messages, out TValue value)
        {
            isSuccessful = IsSuccessful;
            messages = ValidationMessages;
            value = Value;
        }

        public void Deconstruct(out Result validationResult, out TValue value)
        {
            validationResult = Result.Create(ValidationMessages);
            value = Value;
        }

        public Result<TValue> Merge(Result<TValue> result)
        {
            return new Result<TValue>(result.Value, ValidationMessages.Concat(result.ValidationMessages));
        }

        public static implicit operator Result<TValue>(Result result)
        {
            if (result.IsSuccessful)
                throw new NotSupportedException(
                    "Cannot implicitly convert empty result to successful result with value.");

            // value is null here, converted from error result, exclamation mark to supress warning
            return new Result<TValue>(default!, result.ValidationMessages);
        }

        public static implicit operator Result<TValue>(TValue value) => Result.Ok(value);
        public static implicit operator Result<TValue>(ValidationMessage.ValidationMessage error) => Result.Error(error);
        public static implicit operator Result<TValue>(ValidationMessage.ValidationMessage[] errors) =>
            Result.Create<TValue>(default, errors);
    }

    public class Result : ResultType
    {
        private Result(IEnumerable<ValidationMessage.ValidationMessage> validationMessages)
        {
            ValidationMessages = validationMessages;
        }

        private Result(ValidationMessage.ValidationMessage validationMessage)
        {
            ValidationMessages = new[] { validationMessage };
        }

        /// <summary>
        /// Combines two results into one result
        /// </summary>
        public Result Merge(Result result)
        {
            return new Result(ValidationMessages.Concat(result.ValidationMessages));
        }

        public void Deconstruct(out bool isSuccessful, out IEnumerable<ValidationMessage.ValidationMessage> messages)
        {
            isSuccessful = IsSuccessful;
            messages = ValidationMessages;
        }

        public static Result Create(IEnumerable<ValidationMessage.ValidationMessage> validationMessages) =>
            new(validationMessages);

        public static Result<TValue> Create<TValue>(TValue value, IEnumerable<ValidationMessage.ValidationMessage> validationMessages) =>
            Result<TValue>.Create(value, validationMessages);


        public static Result Ok() => new(Enumerable.Empty<ValidationMessage.ValidationMessage>());

        public static Result<TValue> Ok<TValue>(TValue value) => Result<TValue>.Create(value);

        public static Result Error(ValidationKeyDefinition validationKey, object[] parameters)
            => new(ValidationMessage.ValidationMessage.Create(validationKey, parameters));

        public static Result Error(ValidationKeyDefinition validationKey, object parameter)
            => new(ValidationMessage.ValidationMessage.Create(validationKey, parameter));

        public static Result Error(ValidationKeyDefinition validationKey)
            => new(ValidationMessage.ValidationMessage.Create(validationKey));

        public static Result Error(Result result)
            => new Result(result.ValidationMessages);

        public static Result Error(IEnumerable<ValidationMessage.ValidationMessage> validationMessages)
            => new Result(validationMessages);

        public static Result Error(ValidationMessage.ValidationMessage validationMessage)
            => new Result([validationMessage]);

        public static Result<T> Try<T>(Func<T> operation, ValidationKeyDefinition errorKey, params object[] parameters)
        {
            try
            {
                return Ok(operation());
            }
            catch (Exception ex)
            {
                return Error(errorKey, parameters.Concat(new[] { ex.Message }).ToArray());
            }
        }

        public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> operation, ValidationKeyDefinition errorKey,
            params object[] parameters)
        {
            try
            {
                return Ok(await operation());
            }
            catch (Exception ex)
            {
                return Error(errorKey, parameters.Concat(new[] { ex.Message }).ToArray());
            }
        }
        
        public static implicit operator Result(ValidationMessage.ValidationMessage error) => Error(error);
    }
}