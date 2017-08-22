using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Take.Blip.Client.Activation
{
    /// <summary>
    /// Defines a resolver that calls sequentially the passed resolvers.
    /// </summary>
    public sealed class AggregateTypeResolver : ITypeResolver
    {
        private readonly ITypeResolver[] _typeResolvers;

        public AggregateTypeResolver(params ITypeResolver[] typeResolvers)
        {
            _typeResolvers = typeResolvers ?? throw new ArgumentNullException(nameof(typeResolvers));
        }

        public Type Resolve(string typeName) 
            => _typeResolvers
                .Select(typeResolver => typeResolver.Resolve(typeName))
                .FirstOrDefault(type => type != null);
    }
}
