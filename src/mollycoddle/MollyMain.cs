namespace mollycoddle;

using System;
using System.Threading.Tasks;
using Plisky.Diagnostics;
using Plisky.Plumbing;

internal class MollyMain {
    protected Bilge b;
    protected MollyOptions mo;
    protected string? basePathToSave = null;

    public Action<string, OutputType> WriteOutput { get; set; } = (a, b) => { };

    public MollyMain(MollyOptions options, Bilge bilgeInstance) {
        mo = options;
        b = bilgeInstance;
    }

    internal async Task<CheckResult> DoMollly() {
        var result = new CheckResult();
        b.Info.Flow();

        ValidateMollyOptions();

        b.Verbose.Log($"Targeting Directory ]{mo.DirectoryToTarget}");

        await HandleMcRuleSources(mo);
        if (mo.GetCommonFiles) {
            var fetcher = new CommonFilesFetcher(mo, b);
            (int getResult, string savedDirectory) = fetcher.FetchCommonFiles();
            if (getResult == 0) {
                b.Verbose.Log($"Common files fetched successfully and saved to: {savedDirectory}");
            } else {
                b.Verbose.Log($"One or more files failed to download.");
                result.AddDefect("CommonFilesFetchError", $"Failed to fetch common files. {getResult} errors occurred.");
            }
            return result;

        }
        Hub.Current.Launch(new CheckpointMessage { Name = "rules." });

        var ps = new ProjectStructure {
            Root = mo.DirectoryToTarget
        };
        b.Info.Flow("Project Structure");
        ps.PopulateProjectStructure();

        var mrf = new MollyRuleFactory();
        var molly = new Molly(mo);
        b.Info.Flow("Add Project Structure");
        molly.AddProjectStructure(ps);
        try {
            b.Info.Log($"Loading Rules from {mo.RulesFile}");
            molly.ImportRules(mrf.LoadRulesFromFile(mo.RulesFile));
        } catch (InvalidOperationException iox) {
            b.Error.ReportRecord(new ErrorDescription((short)ErrorModule.Main, (short)ErrorCode.ImportMollyRules), $"Error Context: {iox.Message}");
            b.Error.Log($"Exception occured reading rules files |{iox.Message}|");
            WriteOutput($"Error - Unable To Read RulesFiles", OutputType.Error);
            Exception? eox = iox;
            while (eox != null) {
                WriteOutput($"RulesFiles::Error: {eox.Message}", OutputType.Error);
                eox = eox.InnerException;
            }
            throw;
        }

        result = molly.ExecuteAllChecks();
        Hub.Current.Launch(new CheckpointMessage { Name = "checks." });

        string lastWrittenRule = string.Empty;
        foreach (var l in result.ViolationsFound.OrderBy(p => p.RuleName)) {
            if (mo.AddHelpText) {
                if (l.RuleName != lastWrittenRule) {
                    WriteOutput($"❓ {l.RuleName} Further help:  {molly.GetRuleSupportingInfo(l.RuleName)}", OutputType.Info);
                    lastWrittenRule = l.RuleName;
                }
                WriteOutput($"{l.Additional}", OutputType.Violation);
            } else {
                WriteOutput($"{l.RuleName} ({l.Additional})", OutputType.Violation);
            }
        }

        return result;
    }

    private void ValidateMollyOptions() {
        if (!ValidateDirectory(mo.DirectoryToTarget)) {
            WriteOutput($"InvalidCommand:  -Dir Parameter Validation >  Directory Was Not Correct (Does this directory exist? [{mo.DirectoryToTarget}])", OutputType.Error);
            throw new DirectoryNotFoundException($"Directory not found [{mo.DirectoryToTarget}]");
        }

        if (!ValidateRulesFile(mo.RulesFile) && !mo.GetCommonFiles) {
            WriteOutput($"InvalidCommand: -rulesfile parameter validation > RulesFile was not correct (Does this rules file exist? [{mo.RulesFile}])", OutputType.Error);
            throw new FileNotFoundException($"Rules file not found [{mo.RulesFile}]");
        }
    }

    private static bool ValidateRulesFile(string rulesFile) {
        return !string.IsNullOrWhiteSpace(rulesFile);
    }

    private static bool ValidateDirectory(string pathToCheck) {
        return !string.IsNullOrWhiteSpace(pathToCheck) && Directory.Exists(pathToCheck);
    }

    private async Task HandleMcRuleSources(MollyOptions mo) {
        b.Info.Flow();

        var fileManager = new NexusSupport(mo);

        basePathToSave = Path.Combine(mo.TempPath, "mccache");

        if (Directory.Exists(basePathToSave)) {
            b.Verbose.Log($"Removing existing cache directory [{basePathToSave}]");
            try {
                Directory.Delete(basePathToSave, true);
                Directory.CreateDirectory(basePathToSave);
            } catch (IOException) {
                b.Error.Log($"Unable to delete existing cache directory [{basePathToSave}]. Suppressing Error.");
            }
        }

        fileManager.BasePathToSave = basePathToSave;
        mo.RulesFile = await fileManager.ProcessNexusSupport(mo.RulesFile, ProcessKind.RulesFile);
        mo.PrimaryFilePath = await fileManager.ProcessNexusSupport(mo.PrimaryFilePath, ProcessKind.PrimaryFile);
    }
}