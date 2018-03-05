using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.SetBucket
{
    public class SetBucketSettings : IValidable
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public int? Expiration { get; set; }

        public string Document { get; set; }

        public Document ToDocument()
        {
            var mediaType = MediaType.Parse(Type);

            if (mediaType.IsJson)
            {
                var json = JObject.Parse(Document);
                return new JsonDocument(json.ToObject<Dictionary<string, object>>(), mediaType);
            }

            return new PlainDocument(Document, mediaType);
        }

        public void Validate()
        {
            if (Id == null)
            {
                throw new ValidationException($"The '{nameof(Id)}' settings value is required for '{nameof(SetBucket)}' action");
            }
            if (Type == null)
            {
                throw new ValidationException($"The '{nameof(Type)}' settings value is required for '{nameof(SetBucket)}' action");
            }
        }
    }
}