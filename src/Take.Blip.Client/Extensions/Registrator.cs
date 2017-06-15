using System.Reflection;
using Lime.Protocol.Serialization;

namespace Take.Blip.Client.Extensions
{
    public static class Registrator
    {
        public static void RegisterDocuments()
        {
            TypeUtil.RegisterDocuments(typeof(Registrator).GetTypeInfo().Assembly);
        }
    }
}
