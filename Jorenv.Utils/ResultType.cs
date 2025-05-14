using System.Collections.Generic;
using System.Linq;

namespace Jorenv.Utils
{
    public abstract class ResultType
    {
        public bool IsSuccessful => ValidationMessages.All(m => m.SeverityLevel != SeverityLevel.Error);
        public bool IsFailure => !IsSuccessful;
        public IEnumerable<ValidationMessage> ValidationMessages { get; protected set; } = null!;

        public override string ToString()
        {
            return string.Join(", ", ValidationMessages.Select(m => m.TranslationKey.ToString()));
        }

        public string ToStringWithParameters()
        {
            return string.Join(", ", ValidationMessages.Select(vm => vm.MapToErrorMessage()));
        }
    }
}