﻿using System.Collections.Generic;
using System.Linq;

namespace JV.ResultUtilities
{
    public abstract class ResultType
    {
        public bool IsSuccessful => !ValidationMessages.Any();
        public bool IsFailure => !IsSuccessful;
        public IEnumerable<ValidationMessage.ValidationMessage> ValidationMessages { get; protected set; } = null!;

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