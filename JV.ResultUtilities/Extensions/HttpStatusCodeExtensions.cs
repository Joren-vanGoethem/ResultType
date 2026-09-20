using System;
using System.Net;
using JV.ResultUtilities.ValidationMessage;

namespace JV.ResultUtilities.Extensions
{
    /// <summary>
    /// Well-known <see cref="ValidationKeyDefinition.Metadata"/> entry names the library itself defines.
    /// Consumers may add their own; these are the ones the typed helpers read.
    /// </summary>
    public static class ValidationKeyMetadata
    {
        /// <summary>Stores a <see cref="System.Net.HttpStatusCode"/>. Written by <c>WithHttpStatusCode</c>.</summary>
        public const string HttpStatusCode = "http.status";
    }

    /// <summary>
    /// Typed access to the HTTP status a key wants an API to answer with. <see cref="HttpStatusCode"/> is a
    /// base-library type, so declaring the status next to the key in domain code adds no web dependency;
    /// the API layer's exception handler reads it back and no longer needs a per-controller
    /// <c>HasError(key) → NotFound()</c> ladder.
    /// </summary>
    public static class HttpStatusCodeExtensions
    {
        /// <summary>
        /// Returns a copy of <paramref name="definition"/> whose <see cref="ValidationKeyDefinition.Metadata"/>
        /// carries <paramref name="statusCode"/> under <see cref="ValidationKeyMetadata.HttpStatusCode"/>.
        /// </summary>
        public static ValidationKeyDefinition WithHttpStatusCode(this ValidationKeyDefinition definition,
            HttpStatusCode statusCode)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            return definition.WithMetadata(ValidationKeyMetadata.HttpStatusCode, statusCode);
        }

        /// <summary>
        /// Reads the status stored by <see cref="WithHttpStatusCode"/>. Also accepts an <see cref="int"/>
        /// stored directly through <c>WithMetadata</c>, so a key annotated by hand still resolves.
        /// </summary>
        public static bool TryGetHttpStatusCode(this ValidationKeyDefinition definition, out HttpStatusCode statusCode)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            if (definition.TryGetMetadata<HttpStatusCode>(ValidationKeyMetadata.HttpStatusCode, out statusCode))
                return true;

            if (definition.TryGetMetadata<int>(ValidationKeyMetadata.HttpStatusCode, out var numeric))
            {
                statusCode = (HttpStatusCode)numeric;
                return true;
            }

            statusCode = default;
            return false;
        }

        /// <summary>The status the key declares, or null when it declares none.</summary>
        public static HttpStatusCode? GetHttpStatusCode(this ValidationKeyDefinition definition)
            => definition.TryGetHttpStatusCode(out var code) ? code : null;

        /// <summary>Shortcut for <c>message.KeyDefinition.TryGetHttpStatusCode(...)</c>.</summary>
        public static bool TryGetHttpStatusCode(this ValidationMessage.ValidationMessage message, out HttpStatusCode statusCode)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            return message.KeyDefinition.TryGetHttpStatusCode(out statusCode);
        }

        /// <summary>Shortcut for <c>message.KeyDefinition.GetHttpStatusCode()</c>.</summary>
        public static HttpStatusCode? GetHttpStatusCode(this ValidationMessage.ValidationMessage message)
            => message.KeyDefinition.GetHttpStatusCode();

        /// <summary>
        /// The single status an API should answer a failed result with, when its messages declare one.
        /// Picks the numerically highest declared status, so a 5xx outranks a 4xx and a 409 outranks a 404
        /// — the most specific refusal wins over "not found". Messages whose key declares no status are
        /// ignored; returns false on success or when no message declares one, and the caller falls back
        /// to its default (typically 400).
        /// </summary>
        public static bool TryGetHttpStatusCode(this ResultType result, out HttpStatusCode statusCode)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            HttpStatusCode? best = null;
            foreach (var message in result.ValidationMessages)
            {
                if (!message.TryGetHttpStatusCode(out var code)) continue;
                if (best == null || (int)code > (int)best.Value) best = code;
            }

            statusCode = best ?? default;
            return best != null;
        }

        /// <summary>Nullable form of <see cref="TryGetHttpStatusCode(ResultType, out HttpStatusCode)"/>.</summary>
        public static HttpStatusCode? GetHttpStatusCode(this ResultType result)
            => result.TryGetHttpStatusCode(out var code) ? code : null;
    }
}
