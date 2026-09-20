using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JV.ResultUtilities.Exceptions;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.Extensions
{
    public static class ResultExtensions
    {
        public static void ThrowIfFailure(this Result result)
        {
            if (result.IsFailure)
                throw new ResultException(result);
        }

        public static void ThrowIfFailure<TValue>(this Result<TValue> result)
        {
            if (result.IsFailure)
                throw new ResultException(result.ValidationMessages);
        }

        /// <summary>
        /// True when the result failed and at least one of its messages carries <paramref name="key"/>.
        /// Compares on <see cref="ValidationKeyDefinition.Key"/>, so two definitions created for the same
        /// key string match even when they are different instances. Defined on <see cref="ResultType"/>
        /// so one overload serves both <see cref="Result"/> and <see cref="Result{TValue}"/>.
        /// </summary>
        public static bool HasError(this ResultType result, ValidationKeyDefinition key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return result.HasError(key.Key);
        }

        /// <summary>
        /// True when the result failed and at least one of its messages has exactly this key string.
        /// </summary>
        public static bool HasError(this ResultType result, string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return result.IsFailure && result.ValidationMessages.Any(m => KeyOf(m) == key);
        }

        /// <summary>
        /// True when the result failed and at least one message key starts with <paramref name="keyPrefix"/>,
        /// e.g. <c>"Backtest."</c> to ask "did anything about the backtest fail". Ordinal comparison.
        /// </summary>
        public static bool HasErrorWithPrefix(this ResultType result, string keyPrefix)
        {
            if (keyPrefix == null) throw new ArgumentNullException(nameof(keyPrefix));
            return result.IsFailure &&
                   result.ValidationMessages.Any(m => KeyOf(m).StartsWith(keyPrefix, StringComparison.Ordinal));
        }

        /// <summary>
        /// The messages of this result that carry <paramref name="key"/>; empty on success or when none match.
        /// </summary>
        public static IEnumerable<ValidationMessage.ValidationMessage> MessagesFor(this ResultType result,
            ValidationKeyDefinition key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return result.ValidationMessages.Where(m => KeyOf(m) == key.Key);
        }

        private static string KeyOf(ValidationMessage.ValidationMessage message)
            => message.KeyDefinition?.Key ?? message.TranslationKey;
        
        /// <summary>
        /// Combines multiple results into one result
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static Result MergeResults(this IEnumerable<Result> results)
            => results.Aggregate(Result.Ok(), (result1, result2) => result1.Merge(result2));

        /// <summary>
        /// Combines multiple results into a result containing all values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="results"></param>
        /// <returns></returns>
        public static Result<IEnumerable<T>> MergeResults<T>(this IEnumerable<Result<T>> results)
            => results.Aggregate(Result.Ok(Enumerable.Empty<T>()), Merge);

        /// <summary>
        /// Combines multiple asynchronous results into a asynchronous result containing all values
        /// </summary>
        public static async Task<Result> MergeResults(this IEnumerable<Task<Result>> resultTasks)
        {
            var mergedResult = Result.Ok();

            foreach (var resultTask in resultTasks)
            {
                var result = await resultTask;
                mergedResult = mergedResult.Merge(result);
            }

            return mergedResult;
        }

        /// <summary>
        /// Executes the next result when the previous result was successful
        /// </summary>
        /// <param name="result"></param>
        /// <param name="nextResult"></param>
        /// <returns></returns>
        public static Result OnSuccess(this Result result, Func<Result> nextResult)
        {
            if (result.IsFailure)
                return result;

            return nextResult();
        }

        /// <summary>
        /// Loops over results and executes the next result when the previous result was successful
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static Result OnSuccess(this IEnumerable<Result> results)
        {
            var previousResult = Result.Ok();

            foreach (var result in results)
            {
                previousResult = previousResult.OnSuccess(() => result);

                if (previousResult.IsFailure)
                    return previousResult;
            }

            return previousResult;
        }

        /// <summary>
        /// Combines a result with multiple values and a result with single value into a result containing all values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="nextResult"></param>
        /// <returns></returns>
        private static Result<IEnumerable<T>> Merge<T>(this Result<IEnumerable<T>> result, Result<T> nextResult)
        {
            if (result.IsFailure || nextResult.IsFailure)
                return Result.Create(result.ValidationMessages.Concat(nextResult.ValidationMessages));

            return Result.Ok(result.Value.Append(nextResult.Value));
        }

        /// <summary>
        /// Combines multiple asynchronous results into a asynchronous result containing all values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="results"></param>
        /// <returns></returns>
        public static async Task<Result<IEnumerable<T>>> MergeResults<T>(this IEnumerable<Task<Result<T>>> results)
        {
            Result<IEnumerable<T>> mergedResult = Result.Ok(Enumerable.Empty<T>());

            foreach (var result in results)
            {
                var awaitedResult = await result;
                mergedResult = Merge(mergedResult, awaitedResult);
            }

            return mergedResult;
        }

        public static Result<T> Ensure<T>(this Result<T> result,
            Func<T, bool> predicate,
            ValidationKeyDefinition errorKey,
            params object[] parameters)
        {
            if (result.IsFailure) return result;

            return predicate(result.Value)
                ? result
                : Result.Error(errorKey, parameters);
        }

        [Obsolete("Use Ensure instead. Filter is an alias for Ensure and will be removed in a future version.")]
        public static Result<T> Filter<T>(this Result<T> result,
            Func<T, bool> predicate,
            ValidationKeyDefinition errorKey,
            params object[] parameters)
        {
            return result.Ensure(predicate, errorKey, parameters);
        }

        public static Result<T> Do<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSuccessful)
                action(result.Value);
            return result;
        }

        public static async Task<Result<T>> DoAsync<T>(this Result<T> result, Func<T, Task> action)
        {
            if (result.IsSuccessful)
                await action(result.Value);
            return result;
        }

        public static async Task<Result<T>> DoAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> action)
        {
            var result = await resultTask;
            if (result.IsSuccessful)
                await action(result.Value);
            return result;
        }

        public static async Task<Result<T>> Do<T>(this Task<Result<T>> resultTask, Action<T> action)
        {
            var result = await resultTask;
            if (result.IsSuccessful)
                action(result.Value);
            return result;
        }

        /// <summary>
        /// Merges a typed result with a collection of non-generic results, combining all validation messages.
        /// </summary>
        public static Result<T> MergeResults<T>(this Result<T> result, IEnumerable<Result> results)
        {
            return result.Merge(results.ToArray());
        }
    }
}