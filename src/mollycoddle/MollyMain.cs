namespace mollycoddle;

using System;
using System.Threading.Tasks;
using Plisky.Diagnostics;
using Plisky.Plumbing;

internal class MollyMain {
    public bool fixApplied = false;
    protected Bilge b;
    protected string? basePathToSave = null;
    protected MollyOptions mo;

    public MollyMain(MollyOptions options, Bilge bilgeInstance) {
        mo = options;
        b = bilgeInstance;
    }

    public Action<string, OutputType> WriteOutput { get; set; } = (a, b) => { };

    internal async Task<CheckResult> DoMollly() {
        var result = new CheckResult();
        b.Info.Flow();

        ValidateMollyOptions();

        b.Verbose.Log($"Targeting Directory ]{mo.DirectoryToTarget}");

        await HandleMcRuleSources(mo);
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
            b.Error.Log($"Exception occurred reading rules files: {mo.RulesFile}. |{iox.Message}|");
            WriteOutput($"Error - Unable To Read RulesFiles: {mo.RulesFile}", OutputType.Error);
            Exception? eox = iox;
            while (eox != null) {
                WriteOutput($"RulesFiles::Error: {eox.Message}", OutputType.Error);
                eox = eox.InnerException;
            }
            throw;
        }

        result = molly.ExecuteAllChecks();
        Hub.Current.Launch(new CheckpointMessage { Name = "checks." });

        // If requested, attempt to get common files for any violations found
        if (mo.Fix && result.ViolationsFound.Count > 0) {
            var defects = molly.ApplyMollyFix();
            if (defects == null) {
                b.Verbose.Log("No defects were found that could be fixed.");
            } else if (defects.Count > 0) {
                foreach (var def in defects) {
                    result.AddDefect(def.Key, def.Value);
                }
            } else {
                fixApplied = true;
            }
        }

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

    private static bool ValidateDirectory(string pathToCheck) {
        return !string.IsNullOrWhiteSpace(pathToCheck) && Directory.Exists(pathToCheck);
    }

    private static bool ValidateRulesFile(string rulesFile) {
        return !string.IsNullOrWhiteSpace(rulesFile);
    }

    private async Task HandleMcRuleSources(MollyOptions mo) {
        b.Info.Flow();

        var fileManager = new NexusSupport(mo);

        basePathToSave = Path.Combine(mo.TempPath, "mccache");

        if (!Directory.Exists(basePathToSave)) {
            b.Verbose.Log($"Creating cache directory [{basePathToSave}]");
            Directory.CreateDirectory(basePathToSave);
        }

        fileManager.BasePathToSave = basePathToSave;
        mo.RulesFile = await fileManager.ProcessNexusSupport(mo.RulesFile, ProcessKind.RulesFile);

        if (!string.IsNullOrWhiteSpace(mo.PrimaryFilePath)) {
            mo.PrimaryFilePath = await fileManager.ProcessNexusSupport(mo.PrimaryFilePath, ProcessKind.PrimaryFile);
        }
    }

    private void ValidateMollyOptions() {
        if (!ValidateDirectory(mo.DirectoryToTarget)) {
            WriteOutput($"InvalidCommand:  -Dir Parameter Validation >  Directory Was Not Correct (Does this directory exist? [{mo.DirectoryToTarget}])", OutputType.Error);
            throw new DirectoryNotFoundException($"Directory not found [{mo.DirectoryToTarget}]");
        }

        if (!ValidateRulesFile(mo.RulesFile)) {
            WriteOutput($"InvalidCommand: -rulesfile parameter validation > RulesFile was not correct (Does this rules file exist? [{mo.RulesFile}])", OutputType.Error);
            throw new FileNotFoundException($"Rules file not found [{mo.RulesFile}]");
        }
    }
}