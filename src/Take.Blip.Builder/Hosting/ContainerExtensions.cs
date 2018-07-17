using Lime.Messaging;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using SimpleInjector;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Actions.ExecuteScript;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;
using Take.Blip.Builder.Actions.ManageList;
using Take.Blip.Builder.Actions.MergeContact;
using Take.Blip.Builder.Actions.ProcessCommand;
using Take.Blip.Builder.Actions.ProcessHttp;
using Take.Blip.Builder.Actions.Redirect;
using Take.Blip.Builder.Actions.SendCommand;
using Take.Blip.Builder.Actions.SendMessage;
using Take.Blip.Builder.Actions.SendMessageFromHttp;
using Take.Blip.Builder.Actions.SendRawMessage;
using Take.Blip.Builder.Actions.SetBucket;
using Take.Blip.Builder.Actions.SetVariable;
using Take.Blip.Builder.Actions.TrackEvent;
using Take.Blip.Builder.Utils;
using Take.Blip.Builder.Variables;

namespace Take.Blip.Builder.Hosting
{
    public static class ContainerExtensions
    {
        public static Container RegisterBuilder(this Container container)
        {
            container.RegisterSingleton<IFlowManager, FlowManager>();
            container.RegisterSingleton<IStateManager, StateManager>();
            container.RegisterSingleton<IContextProvider, ContextProvider>();
            container.RegisterSingleton<INamedSemaphore, MemoryNamedSemaphore>();
            container.RegisterSingleton<IActionProvider, ActionProvider>();
            container.RegisterSingleton<IDocumentSerializer, DocumentSerializer>();
            container.RegisterSingleton<IEnvelopeSerializer, EnvelopeSerializer>();
            container.RegisterSingleton<IDocumentTypeResolver>(new DocumentTypeResolver().WithMessagingDocuments());
            container.RegisterSingleton<IConfiguration, ConventionsConfiguration>();
            container.RegisterSingleton<IVariableReplacer, VariableReplacer>();
            container.RegisterSingleton<IHttpClient, HttpClientWrapper>();
            container.RegisterCollection<IAction>(
                new[] 
                {
                    typeof(ExecuteScriptAction),
                    typeof(SendMessageAction),
                    typeof(SendMessageFromHttpAction),
                    typeof(SendRawMessageAction),
                    typeof(SendCommandAction),
                    typeof(ProcessCommandAction),
                    typeof(TrackEventAction),
                    typeof(ProcessHttpAction),
                    typeof(ManageListAction),
                    typeof(MergeContactAction),
                    typeof(SetVariableAction),
                    typeof(SetBucketAction),
                    typeof(RedirectAction),
                    typeof(ForwardMessageToDeskAction),
                });
            container.RegisterCollection<IVariableProvider>(
                new[]
                {
                    typeof(BucketVariableProvider),
                    typeof(CalendarVariableProvider),
                    typeof(ContactVariableProvider),
                    typeof(ContextVariableProvider),
                    typeof(RandomVariableProvider),
                    typeof(FlowConfigurationVariableProvider),
                    typeof(InputVariableProvider),
                    typeof(StateVariableProvider),
                });

            return container;
        }
    }
}
