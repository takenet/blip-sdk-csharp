using System;
using System.Collections.Generic;
using System.Text;
using Lime.Protocol;
using Take.Blip.Client.Extensions.Resource;
using Takenet.Iris.Messaging.Contents;

namespace Take.Blip.Client.Activation.Legacy
{
    public class DocumentDefinition
    {
        /// <summary>
        /// Gets or sets the resource key to be used as content for the message.
        /// </summary>
        public string ResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the media type of the message.
        /// </summary>
        /// <value>
        /// The type of the media.
        /// </value>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the plain content definition of the message.
        /// This value is exclusive with the <see cref="JsonContent"/> property.
        /// </summary>
        /// <value>
        /// The content of the plain.
        /// </value>
        public string PlainContent { get; set; }

        /// <summary>
        /// Gets or sets the JSON content definition of the message.
        /// This value is exclusive with the <see cref="PlainContent"/> property.
        /// </summary>
        /// <value>
        /// The content of the json.
        /// </value>
        public IDictionary<string, object> JsonContent { get; set; }

        public Document ToDocument()
        {
            if (!string.IsNullOrWhiteSpace(ResourceKey))
            {
                return new Resource()
                {
                    Key = ResourceKey
                };
            }

            if (MediaType == null) throw new InvalidOperationException($"The '{nameof(MediaType)}' property is not defined");
            var mediaType = Lime.Protocol.MediaType.Parse(MediaType);
            if (mediaType.IsJson)
            {
                if (JsonContent == null) throw new InvalidOperationException($"The '{nameof(JsonContent)}' property is not defined");
                return new JsonDocument(JsonContent, mediaType);
            }
            if (PlainContent == null) throw new InvalidOperationException($"The '{nameof(PlainContent)}' property is not defined");
            return new PlainDocument(PlainContent, mediaType);
        }
    }
}
