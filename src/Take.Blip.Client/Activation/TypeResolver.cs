using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Take.Blip.Client.Activation
{
    public sealed class TypeResolver : ITypeResolver
    {
        public TypeResolver()
            : this(Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? typeof(TypeResolver).GetTypeInfo().Assembly).Location))
        {
            
        }

        public TypeResolver(string workingDir)
            : this(new PathAssemblyProvider(workingDir))
        {
            
        }

        public TypeResolver(IAssemblyProvider assemblyProvider)
        {
            if (assemblyProvider == null) throw new ArgumentNullException(nameof(assemblyProvider));
            LoadedTypes = assemblyProvider.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .ToList();
        }

        public IEnumerable<Type> LoadedTypes { get; }

        public Type Resolve(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));
            var types = LoadedTypes
                .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (types.Length == 1) return types[0];
            if (types.Length == 0) return Type.GetType(typeName, true, true);
            throw new Exception($"There are multiple types named '{typeName}'");
        }
    }
}
