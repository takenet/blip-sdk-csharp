using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Client.Activation
{
    /// <summary>
    /// Defines a type resolver service.
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        /// Resolves a type by its name.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        Type Resolve(string typeName);
    }
}
