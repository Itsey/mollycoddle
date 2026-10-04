namespace mollycoddle;

/// <summary>
/// Holds a match pattern and a set of secondary patterns, for example a match and a list of exceptions.
/// </summary>
public class MatchWithSecondaryMatches(string ptn) {

    /// <summary>
    /// Gets or sets the primary matching pattern
    /// </summary>
    public string PrimaryPattern { get; set; } = ptn;

    /// <summary>
    /// Gets or sets one or more secondary patterns
    /// </summary>
    public string[] SecondaryList { get; set; } = [];
}