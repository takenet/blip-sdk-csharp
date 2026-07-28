using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
using Take.Blip.Builder.Hosting;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.ProcessCommand
{
    public class ProcessCommandAction : IAction
    {
        private readonly ISender _sender;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly IConfiguration _configuration;
        private readonly IBlipLogger _blipMonitoringLogger;

        private const string SERIALIZABLE_PATTERN = @".+[/|\+]json$";
        private const string OUTPUT_VARIABLE_PROPERTY = "variable";

        public ProcessCommandAction(ISender sender, IEnvelopeSerializer envelopeSerializer, IConfiguration configuration, IBlipLogger? blipMonitoringLogger = null)
        {
            _sender = sender;
            _envelopeSerializer = envelopeSerializer;
            _configuration = configuration;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public string Type => nameof(ProcessCommand);

        public string[]? OutputVariables => new[] { OUTPUT_VARIABLE_PROPERTY.ToCamelCase() };

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(ProcessCommandAction)}' action");

            try
            {
                string variable = null;

                if (settings.TryGetValue(OUTPUT_VARIABLE_PROPERTY, out var variableToken))
                {
                    variable = variableToken.ToString().Trim('"');
                }

                var command = ConvertToCommand(settings);
                command.Id = EnvelopeId.NewId();

                var resultCommand = await _sender.ProcessCommandAsync(command, cancellationToken);

                if (!string.IsNullOrWhiteSpace(variable))
                {
                    var resultCommandJson = _envelopeSerializer.Serialize(resultCommand);
                    await context.SetVariableAsync(variable, resultCommandJson, cancellationToken);
                }

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                    ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                    ["uri"] = command.Uri?.ToString(),
                    ["method"] = command.Method.ToString(),
                    ["outputVariable"] = variable,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                    ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
        }

        private Command ConvertToCommand(JObject settings)
        {

            if (settings.TryGetValue(Command.TYPE_KEY, out var type)
                && Regex.IsMatch(type.ToString(), SERIALIZABLE_PATTERN, default, Constants.REGEX_TIMEOUT)
                && settings.TryGetValue(Command.RESOURCE_KEY, out var resource))
            {
                settings.Property(Command.RESOURCE_KEY).Value = JObject.Parse(resource.ToString());
            }

            var command = settings.ToObject<Command>(LimeSerializerContainer.Serializer);
            InsertMetadatasOnCommand(command);

            return command;
        }

        private void InsertMetadatasOnCommand(Command command)
        {
            if (_configuration.ProcessCommandMetadatasToInsert != null && _configuration.ProcessCommandMetadatasToInsert.Count > 0)
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
