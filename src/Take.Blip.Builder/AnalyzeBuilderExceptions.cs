using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Jint.Runtime;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a builder exception analyze service.
    /// </summary>
    public class AnalyzeBuilderExceptions : IAnalyzeBuilderExceptions
    {
        private List<Func<Exception, bool>> _validations;

        /// <inheritdoc />
        public AnalyzeBuilderExceptions()
        {
            _validations = new List<Func<Exception, bool>>()
            {
                ex => ex is ActionProcessingException && ex?.InnerException is ArgumentException,
                ex => ex is ActionProcessingException && ex?.InnerException is ValidationException,
                ex => ex is ActionProcessingException && ex?.InnerException is InternalJavaScriptException,
                ex => ex is OutputProcessingException
            };
        }

        /// <inheritdoc />
        public Exception VerifyFlowConstructionException(Exception ex)
        {
            foreach (var validation in _validations)
            {
                if (validation(ex))
                {
                    return new FlowConstructionException(ex.Message, ex);
                }
            }

            return ex;
        }
    }
}