namespace mollycoddle;

/// <summary>
/// Represents a check of some activity.  By default the rule will say that it is passed - for rules that should be false until some condition
/// is met then ensure to set Passed = false on creation.
/// </summary>
public class CheckEntityBase(string triggeringRule) {

    /// <summary>
    /// Gets or sets additional information that can be used to describe why the rule failed.
    /// </summary>
    public string AdditionalInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets further diagnostic information used to help diagnose problems.
    /// </summary>
    public string DiagnosticDescriptor { get; set; } = nameof(CheckEntityBase);

    /// <summary>
    /// Gets or sets additional supporting URL to provide help for the rule.
    /// </summary>
    public string HelpUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether isInViolation actually indicates if the violation has occured.
    /// </summary>
    public bool IsInViolation { get; set; }

    /// <summary>
    /// Gets or sets the name of the rule that owns this check and therefore can be used to report out rule violations.
    /// </summary>
    public string OwningRuleIdentity { get; set; } = triggeringRule;

    /// <summary>
    /// Gets or sets a value for passed. It is used for rules where it must actively pass in order to determine if its in violaton or not. Defaults to true.
    /// </summary>
    public bool Passed { get; set; } = true;

    /// <summary>
    /// Gets or sets a format string that describes the violation, by default just returns a single string with the value.
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