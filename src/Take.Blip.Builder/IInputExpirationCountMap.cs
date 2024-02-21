using Take.Blip.Builder.Storage;
using Take.Elephant;

namespace Take.Blip.Builder
{
    public interface IInputExpirationCountMap : INumberMap<FromToIdentityInputExpirationPair>, IExpirableKeyMap<FromToIdentityInputExpirationPair, long>
    {

    }
}
