using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions
{
    public interface IAction
    {
        Type SettingsType { get; }

        string ActionName { get; }

        Task ExecuteAsync(IContext context, object settings, CancellationToken cancellationToken);
    }
}
