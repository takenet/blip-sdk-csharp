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
        private const string FLOW_CONSTRUCTION_TAG = "[FlowConstruction]";

        /// <inheritdoc />
        public AnalyzeBuilderExceptions()
        {
            _validations = new List<Func<Exception, bool>>()
            {
                ex => ex is ActionProcessingException && ex.InnerException != null && ex.InnerException is ArgumentException,
                ex => ex is ActionProcessingException && ex.InnerException != null && ex.InnerException is ValidationException,
                ex => ex is ActionProcessingException && ex.InnerException != null && ex.InnerException is JavaScriptException,
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
                    return new FlowConstructionException(CreateFlowConstructionExceptionMessage(ex.Message), ex);
                }
            }

            return ex;
        }

        /// <inheritdoc />
        public string CreateFlowConstructionExceptionMessage(string message)
        {
            return $"{FLOW_CONSTRUCTION_TAG} {message}";        
        }
    }
}