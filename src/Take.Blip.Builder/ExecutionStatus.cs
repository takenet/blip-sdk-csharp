using System;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Represents the status of an execution.
    /// </summary>
    public enum ExecutionStatus
    {
        /// <summary>
        /// The process is not active.
        /// </summary>
        Stopped, 

        /// <summary>
        /// The process is active under execution.
        /// </summary>
        Executing,

        /// <summary>
        /// The process is not active due to cancellation.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The process is not active due to an execution error.
        /// </summary>
        Failed
    }

    public static class ExecutionStatusExtensions
    {
        public static bool CanChangeTo(this ExecutionStatus currentExecutionStatus, ExecutionStatus executionStatus)
        {
            switch (executionStatus)
            {
                case ExecutionStatus.Stopped:
                case ExecutionStatus.Cancelled:
                case ExecutionStatus.Failed:
                    return currentExecutionStatus == ExecutionStatus.Executing;
                case ExecutionStatus.Executing:
                    return currentExecutionStatus != ExecutionStatus.Executing;
                default:
                    throw new ArgumentOutOfRangeException(nameof(executionStatus), executionStatus, null);
            }
        }
    }
}