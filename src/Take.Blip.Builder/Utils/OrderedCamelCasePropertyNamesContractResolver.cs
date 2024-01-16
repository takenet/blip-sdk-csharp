using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Take.Blip.Builder.Utils
{
    public class OrderedCamelCasePropertyNamesContractResolver : DefaultContractResolver
    {

        public OrderedCamelCasePropertyNamesContractResolver()
        {
            base.NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = false,
                OverrideSpecifiedNames = true
            };
        }

        protected override System.Collections.Generic.IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
        {
            var @base = base.CreateProperties(type, memberSerialization);
            var ordered = @base
                .OrderBy(p => p.Order ?? int.MaxValue)
                .ThenBy(p => p.PropertyName)
                .ToList();
            return ordered;
        }
    }
}
