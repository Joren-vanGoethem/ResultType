using System;
using System.Collections.Generic;

namespace JV.Utils.Extensions
{
    public static class ResultTransformationExtensions
    {
        /// <summary>
        /// Maps successful results to different types
        /// </summary>
        public static Result<TResult> Map<TValue, TResult>(
            this Result<TValue> result,
            Func<TValue, TResult> mapper)
        {
            return result.IsSuccessful
                ? Result.Ok(mapper(result.Value))
                : Result.Create<TResult>(default, result.ValidationMessages);
        }

        /// <summary>
        /// Bind/FlatMap for chaining operations that return Results
        /// </summary>
        public static Result<TResult> Bind<TValue, TResult>(
            this Result<TValue> result,
            Func<TValue, Result<TResult>> binder)
        {
            return result.IsSuccessful
                ? binder(result.Value)
                : Result.Create<TResult>(default, result.ValidationMessages);
        }

        /// <summary>
        /// Match pattern for handling both success and failure cases
        /// </summary>
        public static TResult Match<TValue, TResult>(
            this Result<TValue> result,
            Func<TValue, TResult> onSuccess,
            Func<IEnumerable<ValidationMessage>, TResult> onFailure)
        {
            return result.IsSuccessful
                ? onSuccess(result.Value)
                : onFailure(result.ValidationMessages);
        }

        /// <summary>
        /// Match pattern for non-generic Result
        /// </summary>
        public static TResult Match<TResult>(
            this Result result,
            Func<TResult> onSuccess,
            Func<IEnumerable<ValidationMessage>, TResult> onFailure)
        {
            return result.IsSuccessful
                ? onSuccess()
                : onFailure(result.ValidationMessages);
        }
    }
}