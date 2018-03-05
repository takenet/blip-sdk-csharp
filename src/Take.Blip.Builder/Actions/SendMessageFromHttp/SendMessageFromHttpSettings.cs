using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.SendMessageFromHttp
{
    public class SendMessageFromHttpSettings : IValidable
    {
        public string Type { get; set; }

        public MediaType MediaType;

        public Uri Uri { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public void Validate()
        {
            if (Uri == null)
            {
                throw new ValidationException(
                    $"The '{nameof(Uri)}' settings value is required for '{nameof(SendMessageFromHttpAction)}' action");
            }

            if (Type == null)
            {
                throw new ValidationException(
                    $"The '{nameof(Type)}' settings value is required for '{nameof(SendMessageFromHttpAction)}' action");
            }

            if (!MediaType.TryParse(Type, out MediaType))
            {
                throw new ValidationException(
                    $"The '{nameof(Type)}' settings value must be a valid MIME type for '{nameof(SendMessageFromHttpAction)}' action");
            }
        }
    }
}
