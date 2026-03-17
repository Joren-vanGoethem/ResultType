using System;
using System.Collections.Generic;
using System.Linq;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.Exceptions;


[Serializable]
public sealed class ResultException : Exception
{
  public IEnumerable<ValidationMessage.ValidationMessage> ValidationMessages { get; }

  public ResultException(Result result)
    : base(string.Join("; ", result.ValidationMessages.Select(m => m.MapToErrorMessage())))
  {
    ValidationMessages = result.ValidationMessages;
  }

  public ResultException(ValidationMessage.ValidationMessage validationMessage)
    : base(validationMessage.MapToErrorMessage())
  {
    ValidationMessages = [validationMessage];
  }

  public ResultException(ValidationKeyDefinition validationKey)
    : base($"ValidationKey: {validationKey.TranslationKey}")
  {
    ValidationMessages = [ValidationMessage.ValidationMessage.Create(validationKey)];
  }
}


[Serializable]
public sealed class ResultException<TValue> : Exception
{
  public IEnumerable<ValidationMessage.ValidationMessage> ValidationMessages { get; }

  public ResultException(Result<TValue> result)
    : base(string.Join("; ", result.ValidationMessages.Select(m => m.MapToErrorMessage())))
  {
    ValidationMessages = result.ValidationMessages;
  }
}
