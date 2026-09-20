using System.Collections.Generic;
using System.Linq;

namespace JV.ResultUtilities
{
    public abstract class ResultType
    {
        private IReadOnlyList<ValidationMessage.ValidationMessage> _validationMessages = [];

        public bool IsSuccessful => _validationMessages.Count == 0;
        public bool IsFailure => !IsSuccessful;

        /// <summary>
        /// The messages that make this result a failure; empty on success. Always a materialised list:
        /// a lazily built sequence handed to a constructor is copied once, so repeated reads (every
        /// <see cref="IsSuccessful"/> check, every <c>HasError</c>) never re-run the source enumeration.
        /// </summary>
        public IReadOnlyList<ValidationMessage.ValidationMessage> ValidationMessages => _validationMessages;

        /// <summary>Copies <paramref name="messages"/> once and stores the snapshot.</summary>
        protected void SetValidationMessages(IEnumerable<ValidationMessage.ValidationMessage>? messages)
            => _validationMessages = Materialize(messages);

        private static IReadOnlyList<ValidationMessage.ValidationMessage> Materialize(
            IEnumerable<ValidationMessage.ValidationMessage>? messages)
        {
            if (messages == null) return [];
            // Copy even when already a list: a caller-owned List<T> could be mutated after the fact,
            // and a Result must not change its mind about being successful.
            return messages.ToArray();
        }

        public override string ToString()
        {
            return string.Join(", ",
                ValidationMessages.Select(m =>
                    m.KeyDefinition != null ? m.KeyDefinition.Key : m.TranslationKey.ToString()));
        }

        public string ToStringWithParameters()
        {
            return string.Join(", ", ValidationMessages.Select(vm => vm.MapToErrorMessage()));
        }
    }
}