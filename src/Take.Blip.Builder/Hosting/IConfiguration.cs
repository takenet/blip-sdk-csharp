﻿using System;
using System.Collections.Generic;
using static System.Net.WebRequestMethods;

namespace Take.Blip.Builder.Hosting
{
    public interface IConfiguration
    {
        TimeSpan InputProcessingTimeout { get; }

        int RedisDatabase { get; }

        string RedisKeyPrefix { get; }

        int MaxTransitionsByInput { get; }

        int TraceQueueBoundedCapacity { get; }

        int TraceQueueMaxDegreeOfParallelism { get; }

        TimeSpan TraceTimeout { get; }
        
        TimeSpan DefaultActionExecutionTimeout { get; }

        int ExecuteScriptLimitRecursion { get; }

        int ExecuteScriptMaxStatements { get; }

        long ExecuteScriptLimitMemory { get; }

        long ExecuteScriptLimitMemoryWarning { get; }

        TimeSpan ExecuteScriptTimeout { get; }

        string IpsDeniedOnHttpAction { get; }
    }
}