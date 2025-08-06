namespace mollycoddle;

using Plisky.Plumbing;

[CommandLineArguments]
public class MollyCommandLine {
    private const string VERSION_REPLACEMENT_TAG = "XXVERSIONNAMEXX";
    private string actualDirectory;

    public MollyCommandLine() {
        WarningMode = false;
        WarningsIncludeLinks = false;
        Debug = "off";
        actualDirectory = string.Empty;
    }

    [CommandLineArg("debug", FullDescription = "Set to a debug string to write out additional logging and debugging information.")]
    public string Debug { get; set; }

    [CommandLineArg("dir", IsRequired = true, IsSingleParameterDefault = true, Description = "The root to start scanning in")]
    public string DirectoryToTarget {
        get {
            return actualDirectory;
        }
        set {
            actualDirectory = Path.GetFullPath(value);
        }
    }

    [CommandLineArg("disabled", FullDescription = "If Disabled is set the mollycoddle will not execute but will return 0 instead.")]
    public bool Disabled { get; set; }

    [CommandLineArg("formatter", FullDescription = "If set to azdo then azure build pipelines formatting will be used, otherwise plain text output.")]
    public string OutputFormat { get; set; } = "azdo";

    [CommandLineArg("primaryRoot", FullDescription = "If Primary Source files based rules are used this is the location of the primary Root for these files")]
    [CommandLineArg("masterRoot")]  // compat mode.
    public string? PrimaryPath { get; set; }

    [CommandLineArg("version", FullDescription = "If set then will replace {{VER}} in the rules loading path or file with the string value passed.  Defaults to not used.")]
    public string? RulesetVersion { get; set; } = string.Empty;

    [CommandLineArg("rulesfile", FullDescription = "Path to either a mollyset or molly rules file.")]
    public string? RulesFile { get; set; }

    [CommandLineArg("temppath", FullDescription = "Working directory for molly, and cache location. Supports environment variables.")]
    public string TempPath { get; set; }

    [CommandLineArg("warnonly", FullDescription = "If set then MollyCoddle will return zero even if faults are found, but the faults will be outputted.")]
    public bool WarningMode { get; set; }

    [CommandLineArg("addrulehelp", FullDescription = "If set then MollyCoddle will output hyperlinks in the error messages.")]
    public bool WarningsIncludeLinks { get; set; }

    [CommandLineArg("get", FullDescription = "If set, invokes the get command to fetch common files for the specified repository.")]
    public bool GetCommonFiles { get; set; }

    public MollyOptions GetOptions() {
        RulesFile ??= string.Empty;

        if (RulesFile.Contains(VERSION_REPLACEMENT_TAG)) {
            RulesFile = RulesFile.Replace(VERSION_REPLACEMENT_TAG, RulesetVersion);
        }
        RulesFile = Environment.ExpandEnvironmentVariables(RulesFile);

        if (Debug is "off" or "none") {
            Debug = string.Empty;
        }

        var result = new MollyOptions {
            EnableDebug = !string.IsNullOrEmpty(Debug),
            DebugSetting = Debug,

            AddHelpText = WarningsIncludeLinks,
            PrimaryFilePath = PrimaryPath
        };

        if (!string.IsNullOrEmpty(result.PrimaryFilePath)) {
            result.PrimaryFilePath = result.PrimaryFilePath.Replace(VERSION_REPLACEMENT_TAG, RulesetVersion);
            result.PrimaryFilePath = Environment.ExpandEnvironmentVariables(result.PrimaryFilePath);
        }

        if (string.IsNullOrEmpty(TempPath)) {
            TempPath = Path.GetTempPath();
        } else {
            TempPath = Environment.ExpandEnvironmentVariables(TempPath);
        }
        result.TempPath = TempPath;
        if (!string.IsNullOrWhiteSpace(DirectoryToTarget)) {
            result.DirectoryToTarget = DirectoryToTarget.EndsWith("\\") ? DirectoryToTarget[..^1] : DirectoryToTarget;
        }
        OutputFormat = "default";
        result.RulesFile = RulesFile;
        return result;
    }
}