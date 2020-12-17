using System;

namespace Take.Blip.Builder
{
    public class Constants
    {
        /// <summary>
        /// Default header containing the user's Identity
        /// </summary>
        public const string BLIP_USER_HEADER = "X-Blip-User";

        /// <summary>
        /// Default header containing the bot's Identity
        /// </summary>
        public const string BLIP_BOT_HEADER = "X-Blip-Bot";

        /// <summary>
        /// Default timeout for regex
        /// </summary>
        public static TimeSpan REGEX_TIMEOUT = TimeSpan.FromMinutes(2);
    }
}
