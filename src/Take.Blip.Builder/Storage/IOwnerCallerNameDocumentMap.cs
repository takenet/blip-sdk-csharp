using Take.Elephant;

namespace Take.Blip.Builder.Storage
{
    /// <summary>
    /// Defines a service for storing documents for a given owner, caller and name.
    /// It is used in the context implementations.
    /// </summary>
    public interface IOwnerCallerNameDocumentMap : IExpirableKeyMap<OwnerCallerName, StorageDocument>
    {

    }

    public interface ISourceOwnerCallerNameDocumentMap : IOwnerCallerNameDocumentMap
    {

    }

    public interface ICacheOwnerCallerNameDocumentMap : IOwnerCallerNameDocumentMap
    {

    }
}
