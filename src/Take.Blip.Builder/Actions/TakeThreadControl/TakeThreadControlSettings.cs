using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.TakeThreadControl
{
    public class TakeThreadControlSettings : IValidable
    {
        private string _targetRole;
        private bool _targetRoleDefined;

        [JsonProperty("targetRole")]
        public string TargetRole
        {
            get => _targetRole;
            set
            {
                _targetRole = value;
                _targetRoleDefined = true;
            }
        }

        public string Metadata { get; set; }

        public void Validate()
        {
            if (_targetRoleDefined)
            {
                throw new ValidationException(
                    $"The '{nameof(TargetRole)}' setting is not allowed for '{nameof(TakeThreadControl)}' action");
            }
        }
    }
}
