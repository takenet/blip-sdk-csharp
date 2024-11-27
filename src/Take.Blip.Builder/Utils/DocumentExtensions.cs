using System;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Utils
{
    /// <summary>
    /// Provides extension methods for the <see cref="Document"/> class.
    /// </summary>
    public static class DocumentExtensions
    {
        /// <summary>
        /// Converts the <see cref="Document"/> instance to a object of <see cref="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <param name="jsonSerializer"></param>
        /// <returns></returns>
        public static T ToObject<T>(this Document document, JsonSerializer jsonSerializer = null)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document.GetMediaType().IsJson)
            {
                var serializer = jsonSerializer ?? JsonSerializer.CreateDefault();
                return JObject
                    .FromObject(document, serializer)
                    .ToObject<T>(serializer);
            }

            var parseFunc = TypeUtilEx.GetParseFunc<T>();
            return parseFunc(document.ToString());
        }
    }
}
