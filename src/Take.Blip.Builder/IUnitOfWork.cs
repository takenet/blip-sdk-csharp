using System;
using System.Threading.Tasks;
using System.Threading;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a builder exception analyze service.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Confirms Unit Of Work modifications
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CommitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Adds or modifies a new item to the Unit Of Work dictionary of type "set"
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        void SetVariable(string name, string value, TimeSpan expiration);

        /// <summary>
        /// Adds or modifies a new item to the Unit Of Work dictionary of type "delete"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        void DeleteVariable(string name);

        /// <summary>
        /// First, it try to get the context variable using the dictionary, if it doesn't exist, it search the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetContextVariableAsync(string name, CancellationToken cancellationToken);
    }
}