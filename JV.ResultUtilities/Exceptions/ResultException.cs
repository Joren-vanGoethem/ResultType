using System;
using System.Collections.Generic;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.Exceptions;


[Serializable]
public class ResultException : Exception
{
  public IEnumerable<ValidationMessage.ValidationMessage> ValidationMessages { get; }

  public ResultException(Result result)
  {
    ValidationMessages = result.ValidationMessages;
  }

  public ResultException(ValidationMessage.ValidationMessage validationMessage)
  {
    ValidationMessages = [validationMessage];
  }

  public ResultException(ValidationKeyDefinition validaionKey)
  {
    ValidationMessages = [ValidationMessage.ValidationMessage.Create(validaionKey)];
  }
}


[Serializable]
public sealed class ResultException<TValue> : Exception
{
  public IEnumerable<ValidationMessage.ValidationMessage> ValidationMessages { get; }

  public ResultException(Result<TValue> result)
  {
    ValidationMessages = result.ValidationMessages;
  }
}
