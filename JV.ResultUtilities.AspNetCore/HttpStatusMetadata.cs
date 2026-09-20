using System.Net;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.AspNetCore;

/// <summary>
/// Integer conveniences over the core library's typed HTTP-status metadata
/// (<see cref="HttpStatusCodeExtensions.WithHttpStatusCode"/> / <see cref="HttpStatusCodeExtensions.TryGetHttpStatusCode(ValidationKeyDefinition, out HttpStatusCode)"/>).
/// Both spellings read and write the same <see cref="ValidationKeyMetadata.HttpStatusCode"/> entry, so a key
/// declared with either is honoured by <see cref="ResultExceptionHandler"/>. Prefer the enum form in domain
/// code; use these when a status arrives as a number (configuration, a proxied upstream response).
/// </summary>
public static class HttpStatusMetadata
{
    /// <summary>The <see cref="ValidationKeyDefinition.Metadata"/> entry name. Same as <see cref="ValidationKeyMetadata.HttpStatusCode"/>.</summary>
    public const string Key = ValidationKeyMetadata.HttpStatusCode;

    /// <summary>Numeric form of <see cref="HttpStatusCodeExtensions.WithHttpStatusCode"/>.</summary>
    public static ValidationKeyDefinition WithHttpStatus(this ValidationKeyDefinition definition, int statusCode)
    {
        if (statusCode is < 100 or > 599)
            throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Must be an HTTP status code.");

        return definition.WithHttpStatusCode((HttpStatusCode)statusCode);
    }

    /// <summary>Numeric form of <see cref="HttpStatusCodeExtensions.TryGetHttpStatusCode(ValidationKeyDefinition, out HttpStatusCode)"/>.</summary>
    public static bool TryGetHttpStatus(this ValidationKeyDefinition definition, out int statusCode)
    {
        if (definition.TryGetHttpStatusCode(out var code))
        {
            statusCode = (int)code;
            return true;
        }

        statusCode = 0;
        return false;
    }
}
