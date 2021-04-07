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
        public readonly static TimeSpan REGEX_TIMEOUT = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Supported date formats for validations
        /// </summary>
        public readonly static string[] DateValidationFormats = new[] {
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "dd-MM-yyyy",
            "MM-dd-yyyy",
            "dd-MM",
            "dd/MM",
            "MM-dd",
            "MM-dd-yy",
            "dd-MM-yy",
            "yyyy-MM-ddTHH:mm:ssK",
            "yyyy-dd-MMTHH:mm:ssK"
        };

        /// <summary>
        /// Denominator percentage
        /// </summary>
        public const int PERCENTAGE_DENOMINATOR = 100;
    }
}
