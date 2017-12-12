using Lime.Protocol.Serialization;
using SimpleInjector;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Actions.ManageList;
using Take.Blip.Builder.Actions.ProcessHttp;
using Take.Blip.Builder.Utils;

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
                typeof(TrackEventAction),
                typeof(ProcessHttpAction),
                typeof(ManageListAction)
            });
            return container;
        }
    }
}
