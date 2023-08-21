namespace mollycoddle; 

using Plisky.Diagnostics;

/// <summary>
/// A base class for a validator.
/// </summary>
public abstract class ValidatorBase {
    protected Bilge b = new Bilge("mc-validators-base");

    /// <summary>
    /// Creates a ValidatorBase Object
    /// </summary>
    /// <param name="trigger"></param>
    public ValidatorBase(string trigger) {
        TriggeringRule = trigger;
    }

    /// <summary>
    /// The reference identifier of the rule that is being implemented by this validator.
    /// </summary>
    public string TriggeringRule { get; set; }
}