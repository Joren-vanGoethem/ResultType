using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.AspNetCore;

/// <summary>
/// The HTTP status a <see cref="ValidationKeyDefinition"/> should answer with, stored in the key's
/// <see cref="ValidationKeyDefinition.Metadata"/>. Declaring it on the key keeps "this failure is a
/// 404" next to the key instead of in a <c>HasError</c> ladder in every controller.
/// </summary>
public static class HttpStatusMetadata
{
    /// <summary>The <see cref="ValidationKeyDefinition.Metadata"/> entry name.</summary>
    public const string Key = "http.status";

    /// <summary>
    /// Returns a copy of <paramref name="definition"/> whose failures the <see cref="ResultExceptionHandler"/>
    /// answers with <paramref name="statusCode"/>.
    /// </summary>
    public static ValidationKeyDefinition WithHttpStatus(this ValidationKeyDefinition definition, int statusCode)
    {
        if (statusCode is < 100 or > 599)
            throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Must be an HTTP status code.");

        return definition.WithMetadata(Key, statusCode);
    }

    /// <summary>Reads the status declared with <see cref="WithHttpStatus"/>, if any.</summary>
    public static bool TryGetHttpStatus(this ValidationKeyDefinition definition, out int statusCode)
        => definition.TryGetMetadata(Key, out statusCode);
}
