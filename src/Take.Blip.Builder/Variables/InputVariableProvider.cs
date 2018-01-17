using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder.Variables
{
    public class InputVariableProvider : IVariableProvider
    {
        public VariableSource Source => VariableSource.Input;

        public async Task<string> GetVariableAsync(string name, Identity user, CancellationToken cancellationToken)
        {
            var input = RequestContext.Input;
            if (input == null) return null;

            var nameToLower = name.ToLowerInvariant();

            switch (nameToLower)
            {
                case "content":
                    return input.SerializedInput;

                case "type":
                    return input.Input?.GetMediaType()?.ToString();

                case "intent":
                    return await input.GetIntentAsync();

                case "length":
                    return input.SerializedInput?.Length.ToString();
            }

            if (nameToLower.StartsWith("entity."))
            {
                var entityName = nameToLower.Split('.')[1];
                return await input.GetEntityValue(entityName);
            }

            return null;
        }
    }
}
