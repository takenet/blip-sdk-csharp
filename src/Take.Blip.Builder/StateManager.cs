﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Take.Blip.Client.Extensions.Context;

namespace Take.Blip.Builder
{
    public class StateManager : IStateManager
    {
        private const string PREVIOUS_STATE_PREFIX = "previous";
        private const string STATE_ID_KEY = "stateId";
        private const string MASTER_STATE = "master-state";
        private const string CURRENT_FLOW_SESSION_KEY = "currentflowsession";
        public Task<string> GetStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            return context.GetContextVariableAsync(GetStateKey(context.Flow.Id), cancellationToken);
        }

        public Task<string> GetPreviousStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            return context.GetContextVariableAsync(GetPreviousStateKey(context.Flow.Id), cancellationToken);
        }

        public Task<string> GetParentStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            if (context.Flow.Parent == null)
            {
                return null;
            }

            return context.GetContextVariableAsync(GetStateKey(context.Flow.Parent.Id), cancellationToken);
        }

        public Task SetStateIdAsync(IContext context, string stateId, CancellationToken cancellationToken)
        {
            var expiration = context.Flow?.BuilderConfiguration?.StateExpiration ?? default;
            return context.SetVariableAsync(GetStateKey(context.Flow.Id), stateId, cancellationToken, expiration);
        }

        public Task SetPreviousStateIdAsync(IContext context, string previousStateId, CancellationToken cancellationToken)
        {
            return context.SetVariableAsync(GetPreviousStateKey(context.Flow.Id), previousStateId, cancellationToken);
        }

        public Task DeleteStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            return context.DeleteVariableAsync(GetStateKey(context.Flow.Id), cancellationToken);
        }

        public Task DeleteParentStateIdAsync(IContext context, CancellationToken cancellationToken)
        {
            return context.DeleteVariableAsync(GetStateKey(context.Flow.Parent?.Id), cancellationToken);
        }

        public Task DeleteCurrentFlowSessionAsync(IContext context, CancellationToken cancellationToken)
        {
            return context.DeleteVariableAsync(GetCurrentFlowSessionKey(context.Flow.Parent?.Id), cancellationToken);
        }

        public Task DeleteMasterStateAsync(IContext context, CancellationToken cancellationToken) => context.DeleteVariableAsync(MASTER_STATE, cancellationToken);

        public async Task ResetUserState(IContext context, CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                DeleteMasterStateAsync(context, cancellationToken),
                DeleteStateIdAsync(context, cancellationToken),
                DeleteCurrentFlowSessionAsync(context, cancellationToken),
                DeleteParentStateIdAsync(context, cancellationToken));
        }

        private static string GetStateKey(string flowId) => $"{STATE_ID_KEY}@{flowId}";

        private static string GetCurrentFlowSessionKey(string flowId) => $"{CURRENT_FLOW_SESSION_KEY}@{flowId}";

        private static string GetPreviousStateKey(string flowId) => $"{PREVIOUS_STATE_PREFIX}-{STATE_ID_KEY}@{flowId}";
    }
}