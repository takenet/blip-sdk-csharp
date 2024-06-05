using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2.Functions
{
    /// <summary>
    /// Add context functions to the script engine, allowing users to change bot context inside the javascript.
    /// TODO: for the future, allow adding, getting and removing variables in batch. To do it, we must have a new command for batch operations to avoid sending multiple commands.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Context
    {
        private readonly IContext _context;
        private readonly Time _time;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="time"></param>
        /// <param name="cancellationToken"></param>
        public Context(IContext context, Time time, CancellationToken cancellationToken)
        {
            _context = context;
            _time = time;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Sets a variable in the context.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public async Task SetVariableAsync(string key, object value, TimeSpan expiration = default)
        {
            var result =
                await ScriptObjectConverter.ToStringAsync(value, _time, _cancellationToken);

            if (result != null)
            {
                await _context.SetVariableAsync(key, result, _cancellationToken,
                    expiration: expiration);
            }

            await _context.DeleteVariableAsync(key, _cancellationToken);
        }

        /// <summary>
        /// Deletes a variable from the context.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task DeleteVariableAsync(string key)
        {
            return _context.DeleteVariableAsync(key, _cancellationToken);
        }

        /// <summary>
        /// Gets a variable from the context.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<string> GetVariableAsync(string key)
        {
            return _context.GetVariableAsync(key, _cancellationToken);
        }
    }
}