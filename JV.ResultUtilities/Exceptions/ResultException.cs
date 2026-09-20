using System;
using System.Collections.Generic;
using System.Linq;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.Exceptions;


[Serializable]
public sealed class ResultException : Exception
{
  public IReadOnlyList<ValidationMessage.ValidationMessage> ValidationMessages { get; }

  /// <summary>
  /// The translation keys of <see cref="ValidationMessages"/>, in order, for log calls and tests that
  /// branch on which failure occurred without re-deriving it from the messages.
  /// </summary>
  public IReadOnlyList<string> Keys => ValidationMessages.Select(m => m.TranslationKey).ToArray();

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
  
  public ResultException(ValidationMessage.ValidationMessage[] validationMessages)
    : base(string.Join("; ", validationMessages.Select(m => m.MapToErrorMessage())))
  {
    ValidationMessages = validationMessages;
  }
  
  public ResultException(IList<ValidationMessage.ValidationMessage> validationMessages)
    : base(string.Join("; ", validationMessages.Select(m => m.MapToErrorMessage())))
  {
    ValidationMessages = validationMessages.ToArray();
  }
  
  public ResultException(IEnumerable<ValidationMessage.ValidationMessage> validationMessages)
    : this(validationMessages.ToArray())
  {
  }

  public ResultException(ValidationKeyDefinition validationKey)
    : base($"ValidationKey: {validationKey.TranslationKey}")
  {
    ValidationMessages = [ValidationMessage.ValidationMessage.Create(validationKey)];
  }
}
