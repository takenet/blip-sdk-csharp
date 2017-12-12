using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder.UnitTests
{
    public class ContextTests
    {
        public ContextTests()
        {
            ValuesDictionary = new Dictionary<string, string>();
            FlowId = "0";
            User = "user@msging.net";
        }

        public IDictionary<string, string> ValuesDictionary { get; }

        public string FlowId { get; set; }

        public Identity User { get; set; }


        private Context GetTarget()
        {
            return new Context(
                new DictionaryContextExtension(ValuesDictionary),
                FlowId,
                User);
        }


        private class DictionaryContextExtension : IContextExtension
        {            
            public DictionaryContextExtension(IDictionary<string, string> valuesDictionary)
            {
                ValuesDictionary = valuesDictionary;
            }

            public IDictionary<string, string> ValuesDictionary { get; }

            public Task<T> GetVariableAsync<T>(Identity identity, string variableName, CancellationToken cancellationToken) where T : Document
            {
                if (!ValuesDictionary.TryGetValue(variableName, out var variableValue))
                {
                    
                }

                return null;
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
