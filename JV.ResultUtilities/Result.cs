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
        private readonly TValue _value;

        /// <summary>
        /// Gets the value of a successful result.
        /// Throws <see cref="InvalidOperationException"/> if the result is a failure.
        /// </summary>
        public TValue Value
        {
            get
            {
                if (IsFailure)
                    throw new InvalidOperationException(
                        "Cannot access Value on a failed result. Check IsSuccessful before accessing Value, or use Match/Map/Bind.");
                return _value;
            }
        }

        /// <summary>
        /// Gets the value without throwing on failure. For internal use only.
        /// </summary>
        internal TValue UnsafeValue => _value;

        private Result(TValue value, IEnumerable<ValidationMessage.ValidationMessage> validationMessages)
        {
            _value = value;
            ValidationMessages = validationMessages;
        }

        public static Result<TValue> Create(TValue value)
            => new Result<TValue>(value, []);

        public static Result<TValue> Create(TValue value, IEnumerable<ValidationMessage.ValidationMessage> validationMessages)
            => new Result<TValue>(value, validationMessages);

        public Result<TValue> Merge(params Result[] results)
        {
            return new Result<TValue>(_value, ValidationMessages.Concat(results.SelectMany(r => r.ValidationMessages)));
        }

        public void Deconstruct(out bool isSuccessful, out IEnumerable<ValidationMessage.ValidationMessage> messages, out TValue value)
        {
            isSuccessful = IsSuccessful;
            messages = ValidationMessages;
            value = _value;
        }

        public void Deconstruct(out Result validationResult, out TValue value)
        {
            validationResult = Result.Create(ValidationMessages);
            value = _value;
        }

        public Result<TValue> Merge(Result<TValue> result)
        {
            return new Result<TValue>(result._value, ValidationMessages.Concat(result.ValidationMessages));
        }

        /// <summary>
        /// Casts a failed result to a different value type, forwarding all validation messages.
        /// Throws <see cref="InvalidOperationException"/> if the result is successful.
        /// </summary>
        public Result<TResult> Cast<TResult>()
        {
            if (IsSuccessful)
                throw new InvalidOperationException(
                    "Cannot cast a successful result to a different type. Use Map instead.");

            return Result.Create<TResult>(default!, ValidationMessages);
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

        /// <summary>
        /// Creates a failed Result&lt;T&gt; with the specified validation key and no parameters.
        /// </summary>
        public static Result<T> Error<T>(ValidationKeyDefinition validationKey)
            => Create<T>(default!, new[] { ValidationMessage.ValidationMessage.Create(validationKey) });

        /// <summary>
        /// Creates a failed Result&lt;T&gt; with the specified validation key and a single parameter.
        /// </summary>
        public static Result<T> Error<T>(ValidationKeyDefinition validationKey, object parameter)
            => Create<T>(default!, new[] { ValidationMessage.ValidationMessage.Create(validationKey, parameter) });

        /// <summary>
        /// Creates a failed Result&lt;T&gt; with the specified validation key and parameters.
        /// </summary>
        public static Result<T> Error<T>(ValidationKeyDefinition validationKey, object[] parameters)
            => Create<T>(default!, new[] { ValidationMessage.ValidationMessage.Create(validationKey, parameters) });

        public static implicit operator Result(ValidationMessage.ValidationMessage error) => Error(error);
    }
}
