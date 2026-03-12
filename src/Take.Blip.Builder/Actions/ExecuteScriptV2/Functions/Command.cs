using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using Serilog;
using Take.Blip.Builder.Hosting;
using Take.Blip.Client;
using LimeCommand = Lime.Protocol.Command;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2.Functions
{
    /// <summary>
    /// Add Blip command functions to the script engine, allowing users to process LIME commands inside the JavaScript.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Command
    {
        private readonly ISender _sender;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly IConfiguration _configuration;
        private readonly IContext _context;
        private readonly Time _time;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;

        private const string SERIALIZABLE_PATTERN = @".+[/|\+]json$";

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="envelopeSerializer"></param>
        /// <param name="configuration"></param>
        /// <param name="context"></param>
        /// <param name="time"></param>
        /// <param name="logger"></param>
        /// <param name="cancellationToken"></param>
        public Command(
            ISender sender,
            IEnvelopeSerializer envelopeSerializer,
            IConfiguration configuration,
            IContext context,
            Time time,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            _sender = sender;
            _envelopeSerializer = envelopeSerializer;
            _configuration = configuration;
            _context = context;
            _time = time;
            _logger = logger.ForContext("OwnerIdentity", context.OwnerIdentity)
                .ForContext("UserIdentity", context.UserIdentity);
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Processes a Blip command and returns the serialized result.
        /// </summary>
        /// <param name="payload">The command payload as a JavaScript object.</param>
        /// <returns>The serialized command response as a JSON string.</returns>
        public async Task<string> ProcessAsync(object payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload), "The command payload is required");

            _logger.Information("[{Source}] Processing command", "ExecuteScriptV2.Command");

            var jsonString = await ScriptObjectConverter.ToStringAsync(payload, _time, _cancellationToken);

            if (string.IsNullOrWhiteSpace(jsonString))
                throw new ArgumentException("The command payload cannot be empty", nameof(payload));

            JObject settings;
            try
            {
                settings = JObject.Parse(jsonString);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid command payload: {ex.Message}", nameof(payload), ex);
            }

            var command = _convertToCommand(settings);
            command.Id = Lime.Protocol.EnvelopeId.NewId();

            var resultCommand = await _sender.ProcessCommandAsync(command, _cancellationToken);

            return _envelopeSerializer.Serialize(resultCommand);
        }

        private LimeCommand _convertToCommand(JObject settings)
        {
            if (settings.TryGetValue(LimeCommand.TYPE_KEY, out var type)
                && Regex.IsMatch(type.ToString(), SERIALIZABLE_PATTERN, default, Constants.REGEX_TIMEOUT)
                && settings.TryGetValue(LimeCommand.RESOURCE_KEY, out var resource))
            {
                settings.Property(LimeCommand.RESOURCE_KEY).Value = JObject.Parse(resource.ToString());
            }

            var command = settings.ToObject<LimeCommand>(LimeSerializerContainer.Serializer);
            _insertMetadatasOnCommand(command);

            return command;
        }

        private void _insertMetadatasOnCommand(LimeCommand command)
        {
            if (_configuration.ProcessCommandMetadatasToInsert != null &&
                _configuration.ProcessCommandMetadatasToInsert.Count > 0)
            {
                if (command.Metadata is null)
                    command.Metadata = new Dictionary<string, string>();

                var result = command.Metadata
                    .Concat(_configuration.ProcessCommandMetadatasToInsert)
                    .GroupBy(kv => kv.Key)
                    .ToDictionary(k => k.Key, v => v.Last().Value);

                command.Metadata = result;
            }
        }
    }
}
