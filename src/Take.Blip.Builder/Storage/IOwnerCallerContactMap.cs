using Lime.Messaging.Resources;
using Take.Elephant;

namespace Take.Blip.Builder.Storage
{
    /// <summary>
    /// Defines a service for storing contacts for a given owner and caller.
    /// It is used to cache contacts when using contact extension.
    /// </summary>
    public interface IOwnerCallerContactMap : IExpirableKeyMap<OwnerCaller, Contact>
    {
    }
}