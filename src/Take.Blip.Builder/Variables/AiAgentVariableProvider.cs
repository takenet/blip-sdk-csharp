using System;
using System.Threading.Tasks;
using System.Threading;
using Lime.Protocol.Serialization;
using Serilog;
using Take.Blip.Client;
using System.Collections.Generic;
using System.Text.Json;

namespace Take.Blip.Builder.Variables
{
    public class AiAgentVariableProvider : ResourceVariableProviderBase, IVariableProvider
    {
        private ISender _sender;
        private static string APPLICATION_NAME = "functions";
        private static readonly string BUILDER_ADDRESS = "postmaster@builder.msging.net";
        private static readonly string AI_AGENT_OBJECT = "aiagent";
        private static Dictionary<string,string> values;
        private static readonly string AI_AGENT_ALL_OBJECT = "message";
        private readonly ILogger _logger;
        public AiAgentVariableProvider(ISender sender, IDocumentSerializer documentSerializer, ILogger logger)
            : base(sender, documentSerializer, APPLICATION_NAME, logger, BUILDER_ADDRESS)
        {
            _sender = sender;
            _logger = logger;
            values = new Dictionary<string, string> { { "redirect", "redirect" }, 
                                                      { "userMessageId", "user_message_id" },
                                                      { "skill_id", "skill_id" },
                                                      { "task_id", "taks_id" },
                                                      { "taskName", "task_name" },
                                                      { "skillName", "skill_name" },
                                                      { "toolCall_id", "tool_call_id" },
                                                      { "name", "name" },
                                                      { "parameters", "parameters" },
                                                      { "userMessage", "user_message" },
                                                      { "userMessage_id", "user_message_id" },
                                                      { "errorCode", "error_code" },
                                                      { "message", "" },
            };
        }

        public override VariableSource Source => VariableSource.AiAgent;

        public override async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            try
            {
                var stringAiAgentResult = "";
                var aiAgentObject = await context.GetVariableAsync(AI_AGENT_OBJECT, cancellationToken);

                if (!string.IsNullOrEmpty(aiAgentObject))
                {
                    var obj = JsonSerializer.Deserialize<Dictionary<string, object>>(aiAgentObject);

                    if (name == AI_AGENT_ALL_OBJECT)
                    {
                        return aiAgentObject;
                    }

                    if (values.ContainsKey(name))
                    {
                        string fieldName = values[name];
                        if (obj.ContainsKey(fieldName))
                        {
                            stringAiAgentResult = JsonSerializer.Serialize(obj[fieldName]);
                        }
                    }
                }

                return stringAiAgentResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
