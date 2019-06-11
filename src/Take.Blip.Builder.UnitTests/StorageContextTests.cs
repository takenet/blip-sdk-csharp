using SimpleInjector;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Storage.Memory;
using Take.Blip.Builder.Utils;
using Take.Blip.Builder.Variables;

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
            container.Options.AllowOverridingRegistrations = true;
            container.RegisterBuilder();
            container.RegisterSingleton(ContactExtension);
            container.RegisterSingleton(TunnelExtension);
            container.RegisterSingleton(Sender);
            container.RegisterSingleton(CacheOwnerCallerContactMap);

            return new StorageContext(
                User,
                Application,
                Input,
                Flow,
                container.GetAllInstances<IVariableProvider>(),
                OwnerCallerNameDocumentMap);
        }
    }
}