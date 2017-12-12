using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Utils
{
    public interface IVariableReplacer
    {
        Task<string> ReplaceAsync(string value, IContext context, CancellationToken cancellationToken);
    }
}