using System;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;

namespace Take.Blip.Ai.Bot.Monitoring.Abstractions
{
    public sealed class NullBlipLogger : IBlipLogger
    {
        public void MessageProcessing(LogInput input) { }
        public void ActionExecution(LogInput input) { }
        public void UserContext(LogInput input) { }
        public void ConversationalFlow(LogInput input) { }
        public void UserInput(LogInput input) { }
        public void MessageDelivery(LogInput input) { }
        public void MissingInfoLatency(LogInput input) { }
        public void ErrorEvents(LogInput input, Exception exception) { }
    }
}
