namespace mollycoddle {

    using System.Collections.Generic;
    using Plisky.Diagnostics;

    public class CheckResult {
        protected Bilge b = new Bilge("molly-results");

        public CheckResult() {
            ViolationsFound = new List<Violation>();
        }

        public int DefectCount { get; set; }
        public List<Violation> ViolationsFound { get; set; }

        internal void AddDefect(Violation violation) {
            ViolationsFound.Add(violation);
        }

        internal void AddDefect(string owningRuleIdentity, string additional = "") {
            b.Verbose.Log($"Defect added ({owningRuleIdentity}) - ({additional})");
            DefectCount++;
            var v = new Violation(owningRuleIdentity);
            v.Additional = additional;
            AddDefect(v);
        }
    }
}