using SimpleInjector;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Storage.Memory;

namespace Take.Blip.Builder.Hosting
{
    public static class ContainerExtensions
    {
        public static Container RegisterBuilder(this Container container)
        {
            container.RegisterSingleton<IFlowManager, FlowManager>();
            container.RegisterSingleton<IStorageManager, BucketStorageManager>();
            container.RegisterSingleton<IContextProvider, BucketContextProvider>();
            container.RegisterSingleton<INamedSemaphore, MemoryNamedSemaphore>();
            container.RegisterSingleton<IActionProvider, ActionProvider>();
            container.RegisterSingleton<IFlowMessageManager, FlowMessageManager>();
            container.RegisterSingleton<IUserMessageQueueMap, MemoryUserMessageQueueMap>();

            container.RegisterCollection<IAction>(new[] 
            {
                typeof(SendMessageAction),
                typeof(ReceiveMessageAction)
            });
            return container;
        }
    }
}
