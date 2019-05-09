using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Client
{
    public static class Constants
    {
        public const string DEFAULT_DOMAIN = "msging.net";

        public const string DEFAULT_SCHEME = "net.tcp";

        public const string DEFAULT_HOST_NAME = "tcp.msging.net";

        public const int DEFAULT_PORT = 443;

        public const string POSTMASTER = "postmaster";

        public const string DEFAULT_STATE = "default";

        /// <summary>
        /// Default header containing the user's Identity
        /// </summary>
        public const string BLIP_USER_HEADER = "X-Blip-User";

        /// <summary>
        /// Default header containing the bot's Identity
        /// </summary>
        public const string BLIP_BOT_HEADER = "X-Blip-Bot";
    }
}
