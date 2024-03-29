﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    internal class FlowLoader : IFlowLoader
    {
        public Task<Flow> LoadFlowAsync(FlowType flowType, Flow parentFlow, string identifier, CancellationToken cancellationToken)
        {
            //Used when occurs a subflow redirect. Will be implemented in the future.
            throw new NotImplementedException("Loading a subflow from the SDK is not supported yet.");
        }
    }
}
