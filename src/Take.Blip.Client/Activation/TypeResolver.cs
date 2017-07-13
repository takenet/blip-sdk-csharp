using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Take.Blip.Client.Activation
{
    public sealed class TypeResolver : ITypeResolver
    {
        private readonly string _workingDir;

        public TypeResolver()
            : this(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
        {

        }

        public TypeResolver(string workingDir)
        {
            _workingDir = workingDir;
            LoadedTypes = LoadTypes();
        }

        public IEnumerable<Type> LoadedTypes { get; }

        public Type Resolve(string typeName)
        {
            var types = LoadedTypes
                .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (types.Length == 1) return types[0];
            else if (types.Length == 0) return Type.GetType(typeName, true, true);
            else throw new Exception($"There are multiple types named '{typeName}'");
        }

        private IEnumerable<Type> LoadTypes()
            => LoadAssemblies(_workingDir)
            .SelectMany(a => a.GetTypes())
            .ToList();

        private static IEnumerable<Assembly> LoadAssemblies(string path = ".", string searchPattern = "*.dll")
        {
            foreach (var assemblyPath in Directory.GetFiles(path, searchPattern))
            {
                Assembly assembly = null;

                try
                {                    
                    assembly = LoadAssembly(Path.GetFullPath(assemblyPath));
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }

                if (assembly != null) yield return assembly;
            }
        }

        private static Assembly LoadAssembly(string assemblyPath)
        {
#if NETSTANDARD1_6
            return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
#elif NET461
            var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
            return Assembly.Load(assemblyName);
#endif
        }
    }
}
