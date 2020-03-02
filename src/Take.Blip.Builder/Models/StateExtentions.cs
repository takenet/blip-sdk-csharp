using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Extension methods of <see cref="State"/>
    /// </summary>
    public static class StateExtentions
    {
        public static bool HasInputExpiration(this State state)
        {
            return state?.Input?.HasExpiration() == true;
        }
    }
}
