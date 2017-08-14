using System.Collections.Generic;
using System.Reflection;

namespace Take.Blip.Client.Activation
{
    public sealed class AssemblyProvider : IAssemblyProvider
    {
        private readonly Assembly[] _assemblies;

        public AssemblyProvider(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public IEnumerable<Assembly> GetAssemblies() => _assemblies;
    }
}