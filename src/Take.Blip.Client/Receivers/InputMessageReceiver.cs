using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;
using Lime.Messaging.Contents;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Lime.Protocol.Serialization;
using System.Globalization;
using System.Runtime.Serialization;
using Take.Blip.Client.Session;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.Receivers
{
    /// <summary>
    /// Defines a receiver for getting input data with validation from user.
    /// The data is stored in the user session.
    /// </summary>
    public class InputMessageReceiver : IMessageReceiver
    {
        private const string INPUT_SETTINGS_KEY = "InputSettings";

        private readonly ISender _sender;
        private readonly ISessionManager _sessionManager;
        private readonly IStateManager _stateManager;
        private readonly InputSettings _settings;
        private readonly IDocumentSerializer _documentSerializer;

        public InputMessageReceiver(
            ISender sender,
            ISessionManager sessionManager,
            IStateManager stateManager,
            IDictionary<string, object> settings)
        {
            _sender = sender;
            _sessionManager = sessionManager;
            _settings = InputSettings.Parse(settings);
            _settings.Validate();
            _documentSerializer = new DocumentSerializer();
            _stateManager = stateManager;
        }

        public async Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!await ValidateInputAsync(envelope, cancellationToken)) return;

            // Configure for the next receiver
            if (_settings.Validation != null)
            {
                var validationJson = JsonConvert.SerializeObject(_settings, Application.SerializerSettings);
                await _sessionManager.AddVariableAsync(envelope.From, INPUT_SETTINGS_KEY, validationJson, cancellationToken);
            }

            // Set the out state 
            if (_settings.SuccessOutState != null)
            {
                await _stateManager.SetStateAsync(envelope.From.ToIdentity(), _settings.SuccessOutState, cancellationToken);
            }

            // Send the label
            await _sender.SendMessageAsync(_settings.Label.ToDocument(), envelope.From, cancellationToken);
        }

        public async Task<bool> ValidateInputAsync(Message envelope, CancellationToken cancellationToken)
        {
            // Gets the settings from the previous input
            var settingsJson = await _sessionManager.GetVariableAsync(envelope.From, INPUT_SETTINGS_KEY, cancellationToken);
            if (settingsJson == null) return true;

            var inputSettings = JsonConvert.DeserializeObject<InputSettings>(settingsJson, Application.SerializerSettings);
            if (ValidateRule(envelope.Content, inputSettings.Validation))
            {
                // Save the value in the session
                var variableValue = _documentSerializer.Serialize(envelope.Content);
                await _sessionManager.AddVariableAsync(envelope.From, inputSettings.Validation.VariableName, variableValue, cancellationToken);
                return true;
            }

            // Send a validation error message and resend the previous label
            await _sender.SendMessageAsync(inputSettings.Validation.Error ?? "An validation error has occurred", envelope.From, cancellationToken);
            await Task.Delay(250, cancellationToken);
            await _sender.SendMessageAsync(inputSettings.Label.ToDocument(), envelope.From, cancellationToken);
            return false;
        }

        private bool ValidateRule(Document content, InputValidation inputValidation)
        {
            string contentString;

            switch (inputValidation.Rule)
            {
                case InputValidationRule.Text:
                    if (content is PlainText) return true;
                    break;

                case InputValidationRule.Number:
                    contentString = content.ToString();
                    return int.TryParse(contentString, out int result);

                case InputValidationRule.Date:
                    throw new NotSupportedException("Date validation not supported yet");

                case InputValidationRule.Regex:
                    contentString = content.ToString();
                    var regex = new Regex(inputValidation.Regex);
                    return regex.IsMatch(contentString);

                case InputValidationRule.Type:
                    if (content.GetMediaType().Equals(inputValidation.Type)) return true;
                    break;
            }

            return true;
        }
    }

    public class InputSettings
    {
        private static JsonSerializer Serializer = JsonSerializer.Create(Application.SerializerSettings);

        public DocumentDefinition Label { get; set; }

        public InputValidationSettings Validation { get; set; }

        public string Culture { get; set; } = CultureInfo.InvariantCulture.Name;

        public string SuccessOutState { get; set; }

        public static InputSettings Parse(IDictionary<string, object> dictionary)
            => JObject.FromObject(dictionary).ToObject<InputSettings>(Serializer);

        public void Validate()
        {
            if (Label == null) throw new ArgumentException("Label cannot be null");
            if (Validation == null) throw new ArgumentException("Validation cannot be null");
            if (Validation.VariableName == null) throw new ArgumentException("Validation variable name cannot be null");
            if (Validation.Rule == InputValidationRule.Regex
                && Validation.Regex == null)
            {
                throw new ArgumentException("Regex validation cannot be null");
            }

            if (Validation.Rule == InputValidationRule.Type
                && Validation.Type == null)
            {
                throw new ArgumentException("Type validation cannot be null");
            }
        }
    }

    [DataContract]
    public class InputValidationSettings : InputValidation
    {
        [DataMember(Name = "variableName")]
        public string VariableName { get; set; }
    }
}
