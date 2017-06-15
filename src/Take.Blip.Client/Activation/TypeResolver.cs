using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Take.Blip.Client.Activation
{
    public sealed class TypeResolver : ITypeResolver
    {
        private TypeResolver()
        {
            LoadedTypes = new Lazy<IEnumerable<Type>>(LoadTypes);
        }

        public Lazy<IEnumerable<Type>> LoadedTypes { get; }

        public static TypeResolver Instance => new TypeResolver();

        public Type Resolve(string typeName)
        {
            var types = LoadedTypes
                .Value
                .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

            if (types.Count() == 1) return types.First();
            else if (types.Count() == 0) return Type.GetType(typeName, true, true);
            else throw new Exception($"There are multiple types named {typeName}");
        }

        private static IEnumerable<Type> LoadTypes()
            => LoadAssemblies(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
            .SelectMany(a => a.GetTypes())
            .ToList();

        private static IEnumerable<Assembly> LoadAssemblies(string path = ".", string searchPattern = "*.dll")
        {
            foreach (var assemblyPath in Directory.GetFiles(path, searchPattern))
            {
                Assembly assembly = null;

                try
                {
                    assembly = LoadAssembly(assemblyPath);
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
            var assemblyName = System.Runtime.Loader.AssemblyLoadContext.GetAssemblyName(assemblyPath);
            return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
#else
            var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
            return Assembly.Load(assemblyName);
#endif
        }
    }
}
