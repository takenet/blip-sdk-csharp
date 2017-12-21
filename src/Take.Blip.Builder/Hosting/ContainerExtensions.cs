using Lime.Protocol.Serialization;
using SimpleInjector;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Actions.ManageList;
using Take.Blip.Builder.Actions.ProcessHttp;
using Take.Blip.Builder.Actions.SetContactProperty;
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
            container.RegisterSingleton<IConfiguration, ConventionsConfiguration>();
            container.RegisterSingleton<IVariableReplacer, VariableReplacer>();
            container.RegisterSingleton<IHttpClient, HttpClientWrapper>();

            container.RegisterCollection<IAction>(new[] 
            {
                typeof(SendMessageAction),
                typeof(SendCommandAction),
                typeof(ProcessCommandAction),
                typeof(TrackEventAction),
                typeof(ProcessHttpAction),
                typeof(ManageListAction),
                typeof(SetContactPropertyAction)
            });

            container.RegisterCollection<IVariableProvider>(new[]
            {
                typeof(BucketVariableProvider),
                typeof(CalendarVariableProvider),
                typeof(ContactVariableProvider),
                typeof(ContextVariableProvider),
                typeof(RandomVariableProvider)
            });

            return container;
        }
    }
}
