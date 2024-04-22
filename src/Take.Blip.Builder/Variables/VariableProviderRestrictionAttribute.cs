using System;

namespace Take.Blip.Builder.Variables
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    class VariableProviderRestrictionAttribute : Attribute
    {
        public string[] AllowedActions { get; set; } = new string[] { };

        public string[] DeniedActions { get; set; } = new string[] { };
    }
}