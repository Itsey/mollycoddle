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
    /// Decides if links are written out alongside the list of violations
    /// </summary>
    public bool AddHelpText { get; set; }

    /// <summary>
    /// Specifies the level of trace to be enabled as a Plisky.Diagnostics debug string.
    /// </summary>
    public string DebugSetting { get; set; }

    /// <summary>
    /// The directory against which the analysis is run.
    /// </summary>
    public string DirectoryToTarget { get; set; }

    /// <summary>
    /// Determines whether debugging should be enabled.
    /// </summary>
    public bool EnableDebug { get; set; }

    /// <summary>
    /// The Path to the common files for any primary file comparisons that need to be made
    /// </summary>
    public string? PrimaryFilePath { get; set; }

    /// <summary>
    /// The rules file that is to be loaded, either a rules set or a single rules file.
    /// </summary>
    public string RulesFile { get; set; }

    /// <summary>
    /// Working path for caching files etc, used when a non disk based rules and primary source is used.
    /// </summary>
    public string TempPath { get; set; }

    /// <summary>
    /// If set, a fix operation will be applied to attempt to resolve violations.
    /// </summary>
    public bool Fix { get; set; } = false;

}