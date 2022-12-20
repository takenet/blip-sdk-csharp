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
    public class UOWJson : object
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
        private readonly Dictionary<string, UOWJson> _data = new Dictionary<string, UOWJson>();
        private int _count = 0;
        private int _countUOW = 0;
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
                            _countUOW += 1;
                            await _contextExtension.SetTextVariableAsync(_userIdentity, item.Key.ToLowerInvariant(), item.Value.value, cancellationToken, item.Value.expiration);
                            break;
                        case DELETE_METHOD:
                            _countUOW += 1;
                            await _contextExtension.DeleteVariableAsync(_userIdentity, item.Key.ToLowerInvariant(), cancellationToken);
                            break;
                    }
                }
                catch 
                {
                    _ = "Deu ruim";
                }
                finally
                {
                    _data.Remove(item.Key);
                }
            }

            _ = _count + " X " + _countUOW;
        }

        /// <inheritdoc />
        public void SetVariable(string name, string value, TimeSpan expiration)
        {
            _count += 1;
            Add(name, value, SET_METHOD, expiration);
        }

        /// <inheritdoc />
        public void DeleteVariable(string name)
        {
            _count += 1;
            Add(name, "", DELETE_METHOD);
        }

        /// <inheritdoc />
        public async Task<string> GetContextVariableAsync(string name, CancellationToken cancellationToken)
        {
            _count += 1;
            var contextVariable = _data.GetValueOrDefault(name);

            try
            {
                if(contextVariable == null)
                {
                    _countUOW += 1;
                    var textVariable = await _contextExtension.GetTextVariableAsync(_userIdentity, name, cancellationToken);
                    Add(name, textVariable, GET_METHOD);
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

        private void Add(string key, string value, string method, TimeSpan expiration = default)
        {
            var json = new UOWJson()
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