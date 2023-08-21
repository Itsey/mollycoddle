namespace mollycoddle {

    /// <summary>
    /// Represents a check of some activity.  By default the rule will say that it is passed - for rules that should be false until some condition
    /// is met then ensure to set Passed = false on creation.
    /// </summary>
    public class CheckEntityBase {

        public CheckEntityBase(string triggeringRule) {
            OwningRuleIdentity = triggeringRule;
            Passed = true;
        }

        /// <summary>
        /// Additional information that can be used to describe why the rule failed.
        /// </summary>
        public string AdditionalInfo { get; set; } = string.Empty;

        /// <summary>
        /// Further diagnostic information used to help diagnose problems.
        /// </summary>
        public string DiagnosticDescriptor { get; set; } = nameof(CheckEntityBase);

        /// <summary>
        /// Additional supporting URL to provide help for the rule.
        /// </summary>
        public string HelpUrl { get; set; } = string.Empty;

        /// <summary>
        /// IsInViolation actually indicates if the violation has occured.
        /// </summary>
        public bool IsInViolation { get; set; }

        /// <summary>
        /// The name of the rule that owns this check and therefore can be used to report out rule violations.
        /// </summary>
        public string OwningRuleIdentity { get; set; }

        /// <summary>
        /// Passed is used for rules where it must actively pass in order to determine if its in violaton or not. Defaults to true.
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// A format string that describes the violation, by default just returns a single string with the value.
        /// </summary>
        public string ViolationMessageFormat { get; set; } = "{0}";

        /// <summary>
        /// Gets a violation message by using the format specified in ViolationMessageFormat.
        /// </summary>
        /// <returns>A formatted error string, including the additional information</returns>
        public virtual string GetViolationMessage() {
            return string.Format(ViolationMessageFormat, AdditionalInfo);
        }
    }
}