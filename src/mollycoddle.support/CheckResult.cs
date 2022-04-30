namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CheckResult {
        public List<Violation> ViolationsFound { get; set; }

        public int DefectCount { get; set; }

        internal void AddDefect(string owningRuleIdentity, string additional = "") {
            DefectCount++;
            var v = new Violation(owningRuleIdentity);
            v.Additional = additional;
            ViolationsFound.Add(v);
        }

        public CheckResult() {
            ViolationsFound = new List<Violation>();
        }
    }
}
