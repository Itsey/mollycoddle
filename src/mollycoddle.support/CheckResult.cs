namespace mollycoddle; 

using System.Collections.Generic;
using Plisky.Diagnostics;

public class CheckResult {
    protected Bilge b = new Bilge("molly-results");

    public CheckResult() {
        ViolationsFound = new List<Violation>();
    }

    public int DefectCount { get; private set; }
    public List<Violation> ViolationsFound { get; set; }

    public void AddDefect(Violation violation) {
        ViolationsFound.Add(violation);
        DefectCount++;
    }

    public void AddDefect(string owningRuleIdentity, string additional = "") {
        b.Verbose.Log($"Defect added ({owningRuleIdentity}) - ({additional})");

        var v = new Violation(owningRuleIdentity) {
            Additional = additional
        };
        AddDefect(v);
    }
}