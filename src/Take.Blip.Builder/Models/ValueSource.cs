namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Determine the source of a value.
    /// </summary>
    public enum ValueSource
    {   
        /// <summary>
        /// The values comes from the user input.
        /// </summary>
        Input,

        /// <summary>
        /// The value comes from the conversation context.
        /// </summary>
        Context,

        /// <summary>
        /// The value comes from the identified AI intention of the user input.
        /// </summary>
        Intent,

        /// <summary>
        /// The value comes from the identified AI entity of the user input.
        /// </summary>
        Entity
    }
}