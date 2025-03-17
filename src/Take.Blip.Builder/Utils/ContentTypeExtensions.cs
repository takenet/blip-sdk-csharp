using System;
using System.Linq;
using System.Net.Http.Headers;

namespace Take.Blip.Builder.Utils
{
    public static class ContentTypeExtensions
    {
        /// <summary>
        /// Validates if any of the provided Content-Types match the expected media type.
        /// </summary>
        /// <param name="contentType">The Content-Types to check.</param>
        /// <param name="expectedType">The expected media type for the Content-Type (e.g., application/json).</param>
        /// <returns>True if any media type matches the expected one, otherwise false.</returns>
        public static bool IsContentType(this string contentType, string expectedType)
        {
            if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(expectedType))
                return false;

            var mediaTypes = contentType.Split(',')
                                         .Select(type => type.Trim())
                                         .ToList();

            return mediaTypes.Any(type => new MediaTypeHeaderValue(type).MediaType.Equals(expectedType, StringComparison.OrdinalIgnoreCase));
        }
    }
}
