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
        Bucket,

        /// <summary>
        /// Get values from the flow configuration.
        /// </summary>
        Config,

        /// <summary>
        /// Get values from the input.
        /// </summary>
        Input,

        /// <summary>
        /// Get values from the current state.
        /// </summary>
        State,
        
        /// <summary>
        /// Gets values from current tunnel, if applicable.
        /// </summary>
        Tunnel,
    }
}