using System;
using System.Collections.Generic;
using System.Linq;

namespace JV.Utils
{
  public class Result<TValue> : ResultType
  {
    public TValue Value { get; }

    private Result(TValue value, IEnumerable<ValidationMessage> validationMessages)
    {
      Value = value;
      ValidationMessages = validationMessages;
    }

    public static Result<TValue> Create(TValue value)
      => new Result<TValue>(value, []);

    public static Result<TValue> Create(TValue value, IEnumerable<ValidationMessage> validationMessages)
      => new Result<TValue>(value, validationMessages);

    public Result<TValue> Merge(params Result[] results)
    {
      return new Result<TValue>(Value, ValidationMessages.Concat(results.SelectMany(r => r.ValidationMessages)));
    }

    public void Deconstruct(out bool isSuccessful, out IEnumerable<ValidationMessage> messages, out TValue value)
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

    public static implicit operator Result<TValue>(TValue value)
    {
      return Create(value);
    }
  }

  public class Result : ResultType
  {
    private Result(IEnumerable<ValidationMessage> validationMessages)
    {
      ValidationMessages = validationMessages;
    }

    private Result(ValidationMessage validationMessage)
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

    public void Deconstruct(out bool isSuccessful, out IEnumerable<ValidationMessage> messages)
    {
      isSuccessful = IsSuccessful;
      messages = ValidationMessages;
    }

    public static Result Create(IEnumerable<ValidationMessage> validationMessages) =>
      new(validationMessages);

    public static Result<TValue> Create<TValue>(TValue value, IEnumerable<ValidationMessage> validationMessages) =>
      Result<TValue>.Create(value, validationMessages);


    public static Result Ok() => new(Enumerable.Empty<ValidationMessage>());

    public static Result<TValue> Ok<TValue>(TValue value) => Result<TValue>.Create(value);

    public static Result Error(TranslationKeyDefinition translationKey, object[] parameters)
      => new(ValidationMessage.CreateError(translationKey, parameters));
    
    public static Result Error(TranslationKeyDefinition translationKey, object parameter)
      => new(ValidationMessage.CreateError(translationKey, parameter));

    public static Result Error(TranslationKeyDefinition translationKey)
      => new(ValidationMessage.CreateError(translationKey));
    
    public static Result Error(Result result)
      => new Result(result.ValidationMessages);

    public static Result Error(IEnumerable<ValidationMessage> validationMessages)
      => new Result(validationMessages);
  }
}