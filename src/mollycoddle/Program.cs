// See https://aka.ms/new-console-template for more information

using mollycoddle;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Plumbing;

internal class Program {

    private static Action<string, OutputType> WriteOutput = WriteOutputDefault;
    private static bool WarningMode = false;
    private static int Main(string[] args) {
        Bilge? b = null;

        int exitCode = 0;
        var clas = new CommandArgumentSupport();
        clas.ArgumentPrefix = "-";
        clas.ArgumentPostfix = "=";
        var ma = clas.ProcessArguments<MollyCommandLine>(args);

        WarningMode = ma.WarningMode;

        if (string.Equals(ma.OutputFormat, "azdo", StringComparison.OrdinalIgnoreCase)) {
            WriteOutput = WriteOutputAzDo;
        }

        if (ma.Disabled) {
            WriteOutput($"MollyCoddle is disabled, returning success.", OutputType.Verbose);
            exitCode = 0;
            goto TheEndIsNigh;
        }

        var mo = ma.GetOptions();
        if (mo.EnableDebug) {
            ConfigureTrace(mo.DebugSetting);
        }

        b = new Bilge("mollycoddle");
        Bilge.Alert.Online("mollycoddle");
        b.Verbose.Dump(args, "command line arguments");

        string directoryToTarget = mo.DirectoryToTarget;

        b.Verbose.Log($"Targeting Directory ]{directoryToTarget}");

        if ((directoryToTarget == "?") || (directoryToTarget == "/?") || (directoryToTarget.ToLower() == "/help")) {
            Console.WriteLine("MollyCoddle, for when you cant let the babbers code on their own....");
            Console.WriteLine(clas.GenerateHelp(ma, "MollyCoddle"));
            exitCode = 0;
            goto TheEndIsNigh;
        }


        if (!ValidateDirectory(directoryToTarget)) {
            WriteOutput($"InvalidCommand - Directory Was Not Correct (Does this directory exist? [{mo.DirectoryToTarget}])", OutputType.Error);
            exitCode = -1;
            goto TheEndIsNigh;
        }
        if (!ValidateRulesFile(mo.RulesFile)) {
            WriteOutput($"InvalidCommand - RulesFile was not correct (Does this rules file exist? [{mo.RulesFile}])", OutputType.Error);
            exitCode = -2;
            goto TheEndIsNigh;
        }

        var ps = new ProjectStructure {
            Root = directoryToTarget
        };
        ps.PopulateProjectStructure();

        var mrf = new MollyRuleFactory();
        var molly = new Molly(mo);
        molly.AddProjectStructure(ps);
        try {
            molly.ImportRules(mrf.LoadRulesFromFile(mo.RulesFile));
        } catch (InvalidOperationException iox) {
            WriteOutput($"Error - Unable To Read RulesFiles", OutputType.Error);
            Exception? eox = iox;
            while (eox != null) {
                WriteOutput($"Error: {eox.Message}", OutputType.Error);
                eox = eox.InnerException;
            }
            exitCode = -90;
            goto TheEndIsNigh;
        }

        CheckResult cr;
        try {
            cr = molly.ExecuteAllChecks();
            exitCode = cr.DefectCount;
        } catch (Exception ex) {
            WriteOutput(ex.Message, OutputType.Error);
            exitCode = -3;
            goto TheEndIsNigh;
        }

        string lastWrittenRule = string.Empty;
        foreach (var l in cr.ViolationsFound.OrderBy(p => p.RuleName)) {
            if (mo.AddHelpText) {
                if (l.RuleName != lastWrittenRule) {
                    Console.WriteLine($"❓ {l.RuleName} Further help:  {molly.GetRuleSupportingInfo(l.RuleName)}");

                    lastWrittenRule = l.RuleName;
                }
                WriteOutput($"{l.Additional}", OutputType.Violation);
            } else {
                WriteOutput($"{l.RuleName} ({l.Additional})", OutputType.Violation);
            }
        }

        if (cr.DefectCount == 0) {
            WriteOutput($"No Violations, Mollycoddle Pass.", OutputType.EndSuccess);
        } else {
            WriteOutput($"Total Violations {cr.DefectCount}", WarningMode ? OutputType.EndSuccess : OutputType.EndFailure);
        }

        if (WarningMode) {
            b.Info.Log("Warning mode, resetting exit code to zero");
            exitCode = 0;
        }

    TheEndIsNigh:
        // Who doesnt love a good goto, secretly.
        b?.Verbose.Log("Mollycoddle, Exit");
        b?.Flush();
        return exitCode;
    }

    private static void ConfigureTrace(string debugSetting) {
        Bilge.Default.Assert.False(string.IsNullOrEmpty(debugSetting),"The debugSetting can not be empty at this point");
        // TODO: Currently hardcoded trace, move this to configuration.
        Bilge.SetConfigurationResolver(debugSetting);


#if !DEBUG
        Bilge.SetConfigurationResolver((x, y) => {
            return System.Diagnostics.SourceLevels.Verbose;
        });
        
#else         
        Bilge.AddHandler(new ConsoleHandler());
#endif
        Bilge.AddHandler(new TCPHandler("127.0.0.1", 9060, true));
    }


    private static void WriteOutputDefault(string v, OutputType ot) {
        string pfx = "";
        switch (ot) {
            case OutputType.Violation: pfx = "💩 Violation: "; break;
            case OutputType.Error: pfx = "Error: "; break;
            case OutputType.Info: pfx = "Info: "; break;
            case OutputType.EndSuccess: pfx = "😎 Completed."; break;
            case OutputType.EndFailure: pfx = "😢 Completed."; break;
        }
        Console.WriteLine($"{pfx}{v}");
    }

    private static void WriteOutputAzDo(string v, OutputType ot) {
        string pfx = "";
        string errType = WarningMode ? "warning" : "error";

        switch (ot) {
            case OutputType.Violation: pfx = $"##vso[task.logissue type={errType}]Violation: "; break;
            case OutputType.Error: pfx = $"##vso[task.logissue type={errType}]Error: "; break;
            case OutputType.Info: pfx = "##[command]"; break;
            case OutputType.EndSuccess:
                Console.WriteLine($"{v}");
                pfx = "##vso[task.complete result=Succeeded;]";
                break;
            case OutputType.EndFailure:
                Console.WriteLine($"{v}");
                pfx = "##vso[task.complete result=Failed;]";
                break;
        }
        Console.WriteLine($"{pfx}{v}");
    }

    private static bool ValidateRulesFile(string rulesFile) {
        if (string.IsNullOrWhiteSpace(rulesFile)) {
            return false;
        }
        if (!File.Exists(rulesFile)) {
            return false;
        }
        return true;
    }

    private static bool ValidateDirectory(string pathToCheck) {
        if (string.IsNullOrWhiteSpace(pathToCheck)) {
            return false;
        }
        if (!Directory.Exists(pathToCheck)) {
            return false;
        }
        return true;
    }
}