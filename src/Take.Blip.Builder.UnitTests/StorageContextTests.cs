using SimpleInjector;
using System;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Variables;

namespace Take.Blip.Builder.UnitTests
{
    public class StorageContextTests : ContextBaseTests
    {
        protected override void AddVariableValue(string variableName, string variableValue)
        {
            throw new NotImplementedException();
        }

        protected override ContextBase GetTarget()
        {
            var container = new Container();
            container.RegisterBuilder();
            container.RegisterSingleton(ContactExtension);
            container.RegisterSingleton(Sender);

            return new StorageContext(
                User,
                Application,
                Input,
                Flow,
                container.GetAllInstances<IVariableProvider>(),
                null);
        }

    }
}
