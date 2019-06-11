using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Network;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Utils;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder.UnitTests
{
    public class ExtensionContextTests : ContextBaseTests
    {
        public ExtensionContextTests()
        {
            ValuesDictionary = new Dictionary<string, Document>(StringComparer.InvariantCultureIgnoreCase);
            ContextExtension = new DictionaryContextExtension(ValuesDictionary);
        }

        public IContextExtension ContextExtension { get; set; }

        public IDictionary<string, Document> ValuesDictionary { get; }

        protected override void AddVariableValue(string variableName, string variableValue)
        {
            ValuesDictionary.Add(variableName, new PlainText() { Text = variableValue });
        }

        protected override ContextBase GetTarget()
        {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;
            container.RegisterBuilder();
            container.RegisterSingleton(ContactExtension);
            container.RegisterSingleton(TunnelExtension);
            container.RegisterSingleton(ContextExtension);
            container.RegisterSingleton(Sender);
            container.RegisterSingleton(CacheOwnerCallerContactMap);

            return new ExtensionContext(
                User,
                Application,
                Input,
                Flow,
                container.GetAllInstances<IVariableProvider>(),
                ContextExtension);
        }

        private class DictionaryContextExtension : IContextExtension
        {
            public DictionaryContextExtension(IDictionary<string, Document> valuesDictionary)
            {
                ValuesDictionary = valuesDictionary;
            }

            public IDictionary<string, Document> ValuesDictionary { get; }

            public async Task<T> GetVariableAsync<T>(Identity identity, string variableName, CancellationToken cancellationToken) where T : Document
            {
                if (!ValuesDictionary.TryGetValue(variableName, out var variableValue))
                {
                    throw new LimeException(ReasonCodes.COMMAND_RESOURCE_NOT_FOUND, "Not found");
                }

                return (T)variableValue;
            }

            public Task SetVariableAsync<T>(Identity identity, string variableName, T document, CancellationToken cancellationToken,
                TimeSpan expiration = default(TimeSpan)) where T : Document
            {
                throw new NotImplementedException();
            }

            public Task SetGlobalVariableAsync<T>(string variableName, T document, CancellationToken cancellationToken,
                TimeSpan expiration = default(TimeSpan)) where T : Document
            {
                throw new NotImplementedException();
            }

            public Task DeleteVariableAsync(Identity identity, string variableName, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task DeleteGlobalVariableAsync(string variableName, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<DocumentCollection> GetVariablesAsync(Identity identity, int skip = 0, int take = 100,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }

            public Task<DocumentCollection> GetIdentitiesAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }
        }
    }
}