using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions
{
    public interface IActionFactory
    {
        IAction Create(string type, JObject settings);
    }
}
