using System;
using System.Collections.Generic;
using System.Text;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class ExecuteScriptSettings : IValidable
    {
        public string Function { get; set; }

        public string Source { get; set; }

        public string[] InputVariables { get; set; }

        public string OutputVariable { get; set; }
        public void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
