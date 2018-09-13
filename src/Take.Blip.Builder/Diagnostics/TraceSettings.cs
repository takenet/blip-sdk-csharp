using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Diagnostics
{
    public class TraceSettings : IValidable
    {
        public TraceMode Mode { get; set; }

        public TraceTargetType TargetType { get; set; }
        
        public string Target { get; set; }

        public int? SlowThreshold { get; set; }

        public void Validate()
        {
            if (Mode == TraceMode.Disabled) return;
            
        }
    }
}
