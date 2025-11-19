using System;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
namespace Take.Blip.Ai.Bot.Monitoring.Abstractions
{
    public interface IBlipLogger
    {
        void MessageProcessing(LogInput input);
        void ActionExecution(LogInput input);
        void UserContext(LogInput input);
        void ConversationalFlow(LogInput input);
        void UserInput(LogInput input);
        void MessageDelivery(LogInput input);
        void MissingInfoLatency(LogInput input);
        void ErrorEvents(LogInput input, Exception exception);
    }
}
