namespace mollycoddle;

using Plisky.Diagnostics;

/// <summary>
/// A base class for a validator.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidatorBase"/> class.
/// Creates a ValidatorBase Object
/// </remarks>
/// <param name="trigger"></param>
public abstract class ValidatorBase(string trigger) {
    protected Bilge b = new("mc-validators-base");

    /// <summary>
    /// Gets or sets the reference identifier of the rule that is being implemented by this validator.
    /// </summary>
    public string TriggeringRule { get; set; } = trigger;
}