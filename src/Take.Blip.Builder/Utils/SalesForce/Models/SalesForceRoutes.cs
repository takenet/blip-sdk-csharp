namespace Take.Blip.Builder.Utils.SalesForce.Models
{
    /// <summary>
    /// Sales force routes
    /// </summary>
    public static class SalesForceRoutes
    {
        /// <summary>
        /// Refresh token
        /// </summary>
        public const string REFRESH_TOKEN = "/services/oauth2/token";

        /// <summary>
        /// Create lead route
        /// </summary>
        public const string CREATE_LEAD = "/services/data/v52.0/sobjects/Lead";

        /// <summary>
        /// Create a lead company
        /// </summary>
        public const string CREATE_COMPANY = "/services/data/v52.0/sobjects/Account";

        /// <summary>
        /// Create one or more custom fields
        /// </summary>
        public const string CREATE_CUSTOM_FIELD = "/services/data/v52.0/tooling/composite";

        /// <summary>
        /// Get leads
        /// </summary>
        public const string GET_LEADS = "/services/data/v52.0/sobjects/Lead/describe";
    }
}
