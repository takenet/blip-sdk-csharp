using System;
using System.Collections.Generic;
using System.Text;
using Lime.Protocol;
using Takenet.Elephant;

namespace Take.Blip.Builder.Storage
{
    public interface IUserMessageQueueMap : IQueueMap<string, Message>
    {
    }
}
