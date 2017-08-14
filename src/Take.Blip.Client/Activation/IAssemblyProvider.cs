using System.Collections.Generic;
using System.Reflection;

namespace Take.Blip.Client.Activation
{
    /// <summary>
    /// Defines a service for providing assemblies.
    /// </summary>
    public interface IAssemblyProvider
    {
        IEnumerable<Assembly> GetAssemblies();
    }
}