namespace mollycoddle {
    using System.Collections.Generic;

    public class Violation {

        public Violation(string owningRuleIdentity) {
            RuleName = owningRuleIdentity;
        }

        public string Additional { get; internal set; }
        public string RuleName { get; set; }
    }
}