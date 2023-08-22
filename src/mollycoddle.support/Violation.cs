namespace mollycoddle;

/// <summary>
/// Violations indicate failures in the rules compliance.  Each violation has an owning rule identity
/// and some additional data to describe the fault ( usually the file or path that failed ).
/// </summary>
public class Violation {

    public Violation(string owningRuleIdentity) {
        RuleName = owningRuleIdentity;
        Additional = string.Empty;
    }

    /// <summary>
    /// Additional supporting information relating to the violation
    /// </summary>
    public string Additional { get; internal set; }

    /// <summary>
    /// The reference rule name.
    /// </summary>
    public string RuleName { get; set; }
}