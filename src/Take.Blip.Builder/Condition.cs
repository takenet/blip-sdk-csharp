namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a conditions to be evaluated using the conversation context.
    /// </summary>
    public class Condition
    {
        /// <summary>
        /// The variable name of the conversation context to be evaluated. Required.
        /// </summary>
        public string Variable { get; set; }

        /// <summary>
        /// The type of the comparison. Optional. The default value is <see cref="ConditionComparison.Equals"/>.
        /// </summary>
        public ConditionComparison Comparison { get; set; }

        /// <summary>
        /// The value to be used by the comparison with the context value. Required.
        /// </summary>
        public string Value { get; set; }
    }
}
