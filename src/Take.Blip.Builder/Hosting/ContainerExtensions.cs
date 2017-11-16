using System;
using System.Collections.Generic;
using System.Text;
using SimpleInjector;
using Take.Blip.Builder.Actions;

namespace Take.Blip.Builder.Hosting
{
    public static class ContainerExtensions
    {
        public static Container RegisterBuilderHosting(this Container container)
        {
            container.RegisterSingleton<IFlowManager, FlowManager>();
            container.RegisterSingleton<IStorageManager, BucketStorageManager>();
            container.RegisterSingleton<IContextProvider, BucketContextProvider>();
            container.RegisterSingleton<INamedSemaphore, MemoryNamedSemaphore>();
            container.RegisterSingleton<IActionProvider, ActionProvider>();

            return container;
        }

        
        
    }
}
