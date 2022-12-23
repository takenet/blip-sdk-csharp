using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    /// <inheritdoc />
    public class UnitOfWorkItem : object
    {
        /// <inheritdoc />
        public string state { get; set; }

        /// <inheritdoc />
        public string value { get; set; }

        /// <inheritdoc />
        public TimeSpan expiration { get; set; }
    }

    /// <inheritdoc />
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IContextExtension _contextExtension;
        private readonly Identity _userIdentity;
        private readonly Dictionary<string, UnitOfWorkItem> _data = new Dictionary<string, UnitOfWorkItem>();
        const string GET_METHOD = "get";
        const string SET_METHOD = "set";
        const string DELETE_METHOD = "delete";

        /// <inheritdoc />
        public UnitOfWork(
            IContextExtension contextExtension,
            Identity UserIdentity
            )
        {
            _contextExtension = contextExtension;
            _userIdentity = UserIdentity;
        }

        /// <inheritdoc />
        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            foreach(var item in _data)
            {
                try
                {
                    switch (item.Value.state)
                    {
                        case SET_METHOD:
                            await _contextExtension.SetTextVariableAsync(_userIdentity, item.Key.ToLowerInvariant(), item.Value.value, cancellationToken, item.Value.expiration);
                            break;
                        case DELETE_METHOD:
                            await _contextExtension.DeleteVariableAsync(_userIdentity, item.Key.ToLowerInvariant(), cancellationToken);
                            break;
                    }
                }
                catch {}
                finally
                {
                    _data.Remove(item.Key);
                }
            }
        }

        /// <inheritdoc />
        public void SetVariable(string name, string value, TimeSpan expiration)
        {
            AddItemToDictionary(name, value, SET_METHOD, expiration);
        }

        /// <inheritdoc />
        public void DeleteVariable(string name)
        {
            AddItemToDictionary(name, "", DELETE_METHOD);
        }

        /// <inheritdoc />
        public async Task<string> GetContextVariableAsync(string name, CancellationToken cancellationToken)
        {
            var contextVariable = _data.GetValueOrDefault(name);

            try
            {
                if(contextVariable == null)
                {
                    var textVariable = await _contextExtension.GetTextVariableAsync(_userIdentity, name, cancellationToken);
                    AddItemToDictionary(name, textVariable, GET_METHOD);
                    return textVariable;
                } else
                {
                    return contextVariable.value;
                }
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }

        private void AddItemToDictionary(string key, string value, string method, TimeSpan expiration = default)
        {
            var json = new UnitOfWorkItem()
            {
                state = method,
                value = Convert.ToString(value),
                expiration = expiration
            };

            if(!_data.ContainsKey(key))
            {
                _data.Add(key, json);
            } else
            {
                if (method != GET_METHOD)
                {
                    _data[key] = json;
                }
            }
        }
    }
}