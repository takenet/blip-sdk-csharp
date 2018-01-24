using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;

namespace Take.Blip.Builder.Variables
{
    public class InputVariableProvider : IVariableProvider
    {
        private readonly IDocumentSerializer _documentSerializer;
        public VariableSource Source => VariableSource.Input;

        public InputVariableProvider(IDocumentSerializer documentSerializer)
        {
            _documentSerializer = documentSerializer;
        }

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

                case "length":
                    return input.SerializedInput?.Length.ToString();
            }

            if (nameToLower.StartsWith("intent."))
            {
                return await GetIntentVariableAsync(input, nameToLower.Split('.')[1]);
            }

            if (nameToLower.StartsWith("entity."))
            {
                var entityNameAndProperty = nameToLower.Split('.');
                if (entityNameAndProperty.Length < 3) return null;

                return await GetEntityVariableAsync(input, entityNameAndProperty[1], entityNameAndProperty[2]);
            }

            return null;
        }


        private async Task<string> GetIntentVariableAsync(LazyInput input, string intentProperty)
        {
            var intent = await input.GetIntentAsync();
            if (intent == null) return null;

            switch (intentProperty)
            {
                case "id":
                    return intent.Id;

                case "name":
                    return intent.Name;

                case "score":
                    return intent.Score?.ToString(CultureInfo.InvariantCulture);

                case "answer":
                    var document = intent.Answer?.Value;
                    if (document == null) return null;
                    return _documentSerializer.Serialize(document);
            }

            return null;
        }

        private async Task<string> GetEntityVariableAsync(LazyInput input, string entityName, string entityProperty)
        {
            var entity = await input.GetEntityValue(entityName);
            if (entity == null) return null;

            switch (entityProperty)
            {
                case "id":
                    return entity.Id;

                case "name":
                    return entity.Name;

                case "value":
                    return entity.Value;
            }

            return null;
        }
    }
}
