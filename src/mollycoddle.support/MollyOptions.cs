namespace mollycoddle;

/// <summary>
/// MollyOptions are cross cutting options relating to how the program works.
/// </summary>
public class MollyOptions {
    public const string PRIMARYPATHLITERAL = "%COMMONROOT%";

    public MollyOptions() {
        DebugSetting = DirectoryToTarget = RulesFile = TempPath = string.Empty;
    }

    /// <summary>
    /// Gets or sets a value indicating whether links are written out alongside the list of violations
    /// </summary>
    public bool AddHelpText { get; set; }

    /// <summary>
    /// Gets or sets the level of trace to be enabled as a Plisky.Diagnostics debug string.
    /// </summary>
    public string DebugSetting { get; set; }

    /// <summary>
    /// Gets or sets the directory against which the analysis is run.
    /// </summary>
    public string DirectoryToTarget { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether debugging should be enabled.
    /// </summary>
    public bool EnableDebug { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether if set, a fix operation will be applied to attempt to resolve violations.
    /// </summary>
    public bool Fix { get; set; } = false;

    /// <summary>
    /// Gets or sets the Path to the common files for any primary file comparisons that need to be made
    /// </summary>
    public string? PrimaryFilePath { get; set; }

    /// <summary>
    /// Gets or sets the rules file that is to be loaded, either a rules set or a single rules file.
    /// </summary>
    public string RulesFile { get; set; }

    /// <summary>
    /// Gets or sets working path for caching files etc, used when a non disk based rules and primary source is used.
    /// </summary>
    public string TempPath { get; set; }
}