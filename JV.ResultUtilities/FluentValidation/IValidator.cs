using System;
using System.Threading.Tasks;

namespace JV.ResultUtilities.FluentValidation;

/// <summary>
/// Non-generic validator interface for discovering and invoking validators without reflection.
/// </summary>
public interface IValidator
{
    Type ValidatedType { get; }
    ResultType ValidateObject(object instance);
    Task<ResultType> ValidateObjectAsync(object instance);
}
