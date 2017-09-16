using Lime.Protocol;

namespace Take.Blip.Client.Web
{
    public static class CommandExtensions
    {
        public static Command CreateSuccessResponse(this Command requestCommand)
        {
            return new Command
            {
                Id = requestCommand.Id,
                To = requestCommand.From,                
                Method = requestCommand.Method,
                Status = CommandStatus.Success
            };
        }
    }
}