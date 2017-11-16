using System;
using System.Collections.Generic;
using System.Text;
using Lime.Protocol;
using Takenet.Elephant;
using Takenet.Elephant.Memory;

namespace Take.Blip.Builder.Storage.Memory
{
    public class MemoryUserMessageQueueMap : QueueMap<string, Message>, IUserMessageQueueMap
    {
        public MemoryUserMessageQueueMap()
        {
            
        }
    }
}
