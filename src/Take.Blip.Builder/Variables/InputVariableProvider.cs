using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Take.Blip.Client;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

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

        public async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            var input = context.Input;
            if (input == null) return null;

            var nameToLower = name.ToLowerInvariant();

            switch (nameToLower)
            {
                case "content":
                    return input.SerializedContent;

                case "message":
                    return input.SerializedMessage;

                case "type":
                    return input.Content?.GetMediaType()?.ToString();

                case "length":
                    return input.SerializedContent?.Length.ToString();

                case "analysis":
                    return await GetAnalyzedContentAsync(input);
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

            if (nameToLower.StartsWith("contentassistant."))
            {
                return await GetContentAssistantVariableAsync(input, nameToLower.Split('.')[1]);
            }

            if (nameToLower.StartsWith("message."))
            {
                return GetMessageProperty(input.Message, nameToLower.Split('.')[1]);
            }

            return null;
        }

        private async Task<string> GetAnalyzedContentAsync(LazyInput input)
        {
            var analyzedContent = await input.AnalyzedContent;
            return analyzedContent != default(AnalysisResponse) 
                ? _documentSerializer.Serialize(analyzedContent) 
                : default;
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

        private async Task<string> GetContentAssistantVariableAsync(LazyInput input, string contentProperty)
        {
            var content = await input.ContentResult;
            if (content == null || content.Id == null) return null;

            switch (contentProperty)
            {
                case "id":
                    return content.Id;

                case "name":
                    return content.Name;

                case "result":
                    return content.Result.Content.ToString();
            }

            return null;
        }

        private string GetMessageProperty(Message message, string name)
        {
            switch (name)
            {
                case "id":
                    return message.Id;
                
                case "from":
                    return message.From;
                
                case "fromidentity":
                    return message.From?.ToIdentity();

                case "to":
                    return message.To;
                
                case "toidentity":
                    return message.To?.ToIdentity();

                case "pp":
                    return message.Pp;
                
                case "ppidentity":
                    return message.Pp?.ToIdentity();
                
                default:
                    return null;
            }
        }
    }
}
