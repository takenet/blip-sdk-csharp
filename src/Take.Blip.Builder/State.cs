namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a state in the conversation state machine.
    /// </summary>
    public class State
    {
        /// <summary>
        /// Unique id for the state. Required.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Indicates if this is the root state if the user has no active conversation. Optional.
        /// </summary>
        public bool Root { get; set; }

        /// <summary>
        /// Determine the actions that should be executed when a user enters the current state. Optional.
        /// </summary>
        public Action[] Actions { get; set; }

        /// <summary>
        /// Determines the possible outputs and its conditions from the current state. Optional.
        /// Array of <see cref="Output"/>. Optional.
        /// </summary>
        public Output[] Outputs { get; set; }
    }
}
