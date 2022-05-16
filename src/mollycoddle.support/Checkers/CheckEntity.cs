namespace mollycoddle {

    /// <summary>
    /// Represents a check of some activity.  By default the rule will say that it is passed - for rules that should be false until some condition
    /// is met then ensure to set Passed = false on creation.
    /// </summary>
    public class CheckEntity {

        public CheckEntity(string triggeringRule) {
            OwningRuleIdentity = triggeringRule;
            Passed = true;
        }

        /// <summary>
        /// IsInViolation actually indicates if the violation has occured.
        /// </summary>
        public bool IsInViolation { get; set; }

        public string OwningRuleIdentity { get; set; }

        /// <summary>
        /// Passed is used for rules where it must actively pass in order to determine if its in violaton or not. Defaults to true.
        /// </summary>
        public bool Passed { get; set; }

        public string Value { get; set; }
    }
}