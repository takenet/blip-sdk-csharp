using System;

namespace Take.Blip.Builder.Variables
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class VariableProviderRestrictionAttribute : Attribute
    {
        public string[] AllowedActions { get; set; } = new string[] { };
    }
}