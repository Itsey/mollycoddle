namespace mollycoddle {

    using System;
    using Minimatch;

    /// <summary>
    /// This performs a basic check using a single Minmatch to determin whether the action called perform check should be used
    /// to perform the code check.  Essentially if the rule can be described using a single minmatch and a single action based
    /// on that minmatch this check entity can be used to validate the rule.
    /// </summary>
    public class MinmatchActionCheckEntity : CheckEntityBase {

        public MinmatchActionCheckEntity(string owningRuleName) : base(owningRuleName) {
            PerformCheck = (entity, fileName) => {
                IsInViolation = true;
                AdditionalInfo = "Default Failure Reason, this is an invalid rule.";
            };
            DoesMatch = new Minimatcher(string.Empty);
        }

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