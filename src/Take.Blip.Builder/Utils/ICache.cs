using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Builder.Utils
{
    public interface ICache<TItem>
    {
        TItem Get(object key);

        void Set(object key, TItem value, TimeSpan cacheExpiration);

        void Remove(object key);
    }
}