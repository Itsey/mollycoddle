namespace mollycoddle {

    /// <summary>
    /// A base class for a validator.
    /// </summary>
    public abstract class ValidatorBase {

        /// <summary>
        /// Creates a ValidatorBase Object
        /// </summary>
        /// <param name="trigger"></param>
        public ValidatorBase(string trigger) {
            TriggeringRule = trigger;
            
        }

        /// <summary>
        /// The reference identifier of the rule that is being implemented by this validator.
        /// </summary>
        public string TriggeringRule { get; set; }


    }
}