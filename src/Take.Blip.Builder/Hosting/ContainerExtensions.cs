using SimpleInjector;
using Take.Blip.Builder.Actions;


namespace Take.Blip.Builder.Hosting
{
    public static class ContainerExtensions
    {
        public static Container RegisterBuilder(this Container container)
        {
            container.RegisterSingleton<IFlowManager, FlowManager>();
            container.RegisterSingleton<IStorageManager, StorageManager>();
            container.RegisterSingleton<IContextProvider, ContextProvider>();
            container.RegisterSingleton<INamedSemaphore, MemoryNamedSemaphore>();
            container.RegisterSingleton<IActionProvider, ActionProvider>();

            container.RegisterCollection<IAction>(new[] 
            {
                typeof(SendMessageAction),
            });
            return container;
        }
    }
}
