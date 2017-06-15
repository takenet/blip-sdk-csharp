using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Take.Blip.Client.Activation
{
    /// <summary>
    /// Defines a factory for instance of <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFactory<T> where T : class
    {
        /// <summary>
        /// Creates an instance of <see cref="T"/>.
        /// </summary>
        /// <param name="serviceProvider">A service provider to allow resolving references.</param>
        /// <param name="settings">A settings dictionary.</param>
        /// <returns></returns>
        Task<T> CreateAsync(IServiceProvider serviceProvider, IDictionary<string, object> settings);
    }
}
