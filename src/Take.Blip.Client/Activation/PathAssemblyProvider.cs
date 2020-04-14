using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Take.Blip.Client.Activation
{
    /// <summary>
    /// Gets all assemblies in a given path.
    /// </summary>
    public sealed class PathAssemblyProvider : IAssemblyProvider
    {
        private readonly string _path;

        public PathAssemblyProvider(string path)
        {
            _path = path;
        }

        public IEnumerable<Assembly> GetAssemblies()
        {
            foreach (var assemblyPath in Directory.GetFiles(_path, "*.dll"))
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
            var fileName = Path.GetFileNameWithoutExtension(assemblyPath);
            var runtimeLibrary = Microsoft.Extensions.DependencyModel.DependencyContext.Default.RuntimeLibraries.FirstOrDefault(l => l.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (runtimeLibrary != null) return Assembly.Load(new AssemblyName(runtimeLibrary.Name));
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        }
    }
}