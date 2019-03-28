using Lime.Messaging.Contents;
using NSubstitute;
using Serilog;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Storage.Memory;
using Take.Blip.Builder.Utils;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.UnitTests
{
    public class StorageContextTests : ContextBaseTests
    {
        public StorageContextTests()
        {
            OwnerCallerNameDocumentMap = new OwnerCallerNameDocumentMap();
        }

        public IOwnerCallerNameDocumentMap OwnerCallerNameDocumentMap { get; set; }

        protected override void AddVariableValue(string variableName, string variableValue)
        {
            var storageDocument = new StorageDocument()
            {
                Type = "text/plain",
                Document = variableValue
            };

            OwnerCallerNameDocumentMap.TryAddAsync(
                new OwnerCallerName()
                {
                    Owner = Application,
                    Caller = User,
                    Name = variableName.ToLowerInvariant()
                },
                storageDocument)
                .Wait();
        }

        protected override ContextBase GetTarget()
        {
            var container = new Container();
            container.RegisterBuilder();
            container.RegisterSingleton(ContactExtension);
            container.RegisterSingleton(Sender);

            var variableProviders = container.GetAllInstances<IVariableProvider>().Where(vp => vp.GetType() != typeof(ContactVariableProvider)).ToList();
            variableProviders.Add(new ContactVariableProvider(CacheContactExtensionDecorator));

            return new StorageContext(
                User,
                Application,
                Input,
                Flow,
                variableProviders,
                OwnerCallerNameDocumentMap);
        }
    }
}