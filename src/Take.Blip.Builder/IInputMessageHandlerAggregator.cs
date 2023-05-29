using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Builder
{
    public interface IInputMessageHandlerAggregator: IInputMessageHandler
    {
        IEnumerable<IInputMessageHandler> GetHandlers();
    }
}
