using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class ExecuteScriptSettings
    {
        public string Function { get; set; }

        public string Source { get; set; }

        public string[] InputVariables { get; set; }

        public string OutputVariable { get; set; }
    }
}
