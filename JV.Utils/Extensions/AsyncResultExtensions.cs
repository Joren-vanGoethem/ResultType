using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JV.Utils.Extensions;

public static class AsyncResultExtensions
{
  public static async Task<Result<TResult>> MapAsync<TValue, TResult>(
    this Task<Result<TValue>> resultTask,
    Func<TValue, Task<TResult>> mapper)
  {
    var result = await resultTask;
    if (result.IsFailure)
      return Result.Create<TResult>(default, result.ValidationMessages);

    var mappedValue = await mapper(result.Value);
    return Result.Ok(mappedValue);
  }

  public static async Task<Result<TResult>> BindAsync<TValue, TResult>(
    this Task<Result<TValue>> resultTask,
    Func<TValue, Task<Result<TResult>>> binder)
  {
    var result = await resultTask;
    return result.IsSuccessful
      ? await binder(result.Value)
      : Result.Create<TResult>(default, result.ValidationMessages);
  }
  
  /// <summary>
  /// Maps a Result to async operation returning Task&lt;Result&lt;TResult&gt;&gt;
  /// </summary>
  public static async Task<Result<TResult>> MapAsync<TValue, TResult>(
    this Result<TValue> result,
    Func<TValue, Task<TResult>> mapper)
  {
    if (result.IsFailure)
      return Result.Create<TResult>(default, result.ValidationMessages);
            
    var mappedValue = await mapper(result.Value);
    return Result.Ok(mappedValue);
  }

  /// <summary>
  /// Binds a Result to async operation returning Task&lt;Result&lt;TResult&gt;&gt;
  /// </summary>
  public static async Task<Result<TResult>> BindAsync<TValue, TResult>(
    this Result<TValue> result,
    Func<TValue, Task<Result<TResult>>> binder)
  {
    return result.IsSuccessful 
      ? await binder(result.Value)
      : Result.Create<TResult>(default, result.ValidationMessages);
  }
  
  // Rename sync versions to avoid ambiguity
  public static async Task<Result<TResult>> BindSync<TValue, TResult>(
    this Task<Result<TValue>> resultTask,
    Func<TValue, Result<TResult>> binder)
  {
    var result = await resultTask;
    return result.IsSuccessful
      ? binder(result.Value)
      : Result.Create<TResult>(default, result.ValidationMessages);
  }

  public static Task<Result<TResult>> BindSync<TValue, TResult>(
    this Result<TValue> result,
    Func<TValue, Result<TResult>> binder)
  {
    var bindResult = result.IsSuccessful 
      ? binder(result.Value)
      : Result.Create<TResult>(default, result.ValidationMessages);
        
    return Task.FromResult(bindResult);
  }

  public static async Task<TResult> MatchAsync<TValue, TResult>(
    this Task<Result<TValue>> resultTask,
    Func<TValue, Task<TResult>> onSuccess,
    Func<IEnumerable<ValidationMessage>, Task<TResult>> onFailure)
  {
    var result = await resultTask;
    return result.IsSuccessful
      ? await onSuccess(result.Value)
      : await onFailure(result.ValidationMessages);
  }

  // Match async with sync handlers
  public static async Task<TResult> MatchAsync<TValue, TResult>(
    this Task<Result<TValue>> resultTask,
    Func<TValue, TResult> onSuccess,
    Func<IEnumerable<ValidationMessage>, TResult> onFailure)
  {
    var result = await resultTask;
    return result.IsSuccessful
      ? onSuccess(result.Value)
      : onFailure(result.ValidationMessages);
  }

  // Match for Result<T> with async handlers
  public static async Task<TResult> MatchAsync<TValue, TResult>(
    this Result<TValue> result,
    Func<TValue, Task<TResult>> onSuccess,
    Func<IEnumerable<ValidationMessage>, Task<TResult>> onFailure)
  {
    return result.IsSuccessful
      ? await onSuccess(result.Value)
      : await onFailure(result.ValidationMessages);
  }

  // For non-generic Result
  public static async Task<TResult> MatchAsync<TResult>(
    this Task<Result> resultTask,
    Func<Task<TResult>> onSuccess,
    Func<IEnumerable<ValidationMessage>, Task<TResult>> onFailure)
  {
    var result = await resultTask;
    return result.IsSuccessful
      ? await onSuccess()
      : await onFailure(result.ValidationMessages);
  }
}