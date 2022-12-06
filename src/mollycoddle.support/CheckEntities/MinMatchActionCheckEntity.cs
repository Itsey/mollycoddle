namespace mollycoddle {

    using System;
    using Minimatch;

    /// <summary>
    /// This performs a basic check using a single Minmatch to determin whether the action called perform check should be used
    /// to perform the code check.  Essentially if the rule can be described using a single minmatch and a single action based
    /// on that minmatch this check entity can be used to validate the rule.
    /// </summary>
    public class MinmatchActionCheckEntity : CheckEntityBase {

        /// <summary>
        /// A format string that describes the violation, by default just returns a single string with the value.
        /// </summary>
        public string ViolationMessageFormat { get; set; } = "{0}";

        /// <summary>
        /// Gets a violation message by using the format specified in ViolationMessageFormat.
        /// </summary>
        /// <param name="supportingInfo">Any additional supporting information to help the engineer identify the issue</param>
        /// <returns>A formatted error string, including the additional information</returns>
        public virtual string GetViolationMessage() {

            return string.Format(ViolationMessageFormat, AdditionalInfo);
        }

        public MinmatchActionCheckEntity(string owningRuleName) : base(owningRuleName) {
        }

        public string HelpUrl { get; set; }
        public string AdditionalInfo { get; set; } = string.Empty;
        public Minimatcher DoesMatch { get; internal set; }
        public Action<MinmatchActionCheckEntity, string> PerformCheck { get; set; }

        public bool ExecuteCheckWasViolation(string fileName) {
            if (IsInViolation) { return false; }

            if (DoesMatch.IsMatch(fileName)) {
                PerformCheck(this, fileName);
            }
            return IsInViolation;
        }
    }
}