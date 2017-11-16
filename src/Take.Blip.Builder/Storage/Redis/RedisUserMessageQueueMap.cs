using System;
using System.Collections.Generic;
using System.Text;
using Lime.Protocol;
using Take.Blip.Builder.Hosting;
using Takenet.Elephant;
using Takenet.Elephant.Redis;

namespace Take.Blip.Builder.Storage.Redis
{
    public class RedisUserMessageQueueMap : RedisQueueMap<string, Message>, IUserMessageQueueMap
    {
        public RedisUserMessageQueueMap(IConfiguration configuration, ISerializer<Message> messageSerializer)
            : base("user-messages", configuration.RedisStorageConfiguration, messageSerializer)
        {
            
        }
    }    
}
