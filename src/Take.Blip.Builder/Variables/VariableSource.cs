namespace Take.Blip.Builder.Variables
{
    public enum VariableSource
    {
        /// <summary>
        /// Contexts variables.
        /// </summary>
        Context,

        /// <summary>
        /// Contact variables.
        /// </summary>
        Contact,

        /// <summary>
        /// Calendar variables.
        /// </summary>
        Calendar,

        /// <summary>
        /// Random values generator.
        /// </summary>
        Random,

        /// <summary>
        /// Get values from bucket.
        /// </summary>
        Bucket
    }
}