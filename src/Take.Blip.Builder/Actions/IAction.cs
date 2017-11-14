using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions
{
    public interface IAction
    {
        Task<bool> CanExecuteAsync(IContext context, CancellationToken cancellationToken);

        Task ExecuteAsync(IContext context, CancellationToken cancellationToken);
    }
}
