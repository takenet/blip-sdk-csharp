using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    public interface IUserOwnerResolver
    {
        Task<UserOwner> GetUserOwnerIdentitiesAsync(Message message, BuilderConfiguration builderConfiguration, CancellationToken cancellationToken);
    }
}