// See https://aka.ms/new-console-template for more information

using mollycoddle;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Plumbing;

internal class Program {
    
    private static Action<string, OutputType> WriteOutput = WriteOutputDefault;

    private static int Main(string[] args) {
        int exitCode = 0;
        var clas = new CommandArgumentSupport();
        clas.ArgumentPrefix = "-";
        clas.ArgumentPostfix = "=";
        var ma = clas.ProcessArguments<MollyCommandLine>(args);
       
        if (string.Equals(ma.OutputFormat, "azdo", StringComparison.OrdinalIgnoreCase)) {
            WriteOutput = WriteOutputAzDo;
        }

        if (ma.Disabled) {
            WriteOutput($"MollyCoddle is disabled, returning success.", OutputType.Verbose);
            return 0;
        }
       
        var mo = ma.GetOptions();
        

        // TODO: Currently hardcoded trace, move this to configuration.
        Bilge.SetConfigurationResolver((x, y) => {
            return System.Diagnostics.SourceLevels.Verbose;
        });
        var b = new Bilge("mollycoddle");
        b.AddHandler(new TCPHandler("127.0.0.1", 9060, true));
        Bilge.Alert.Online("mollycoddle");
        b.Verbose.Dump(args, "command line arguments");

        string directoryToTarget = mo.DirectoryToTarget;

        b.Verbose.Log($"targetting {directoryToTarget}");
        if (!ValidateDirectory(directoryToTarget)) {
            WriteOutput($"InvalidCommand - Directory Was Not Correct [{mo.DirectoryToTarget}]",OutputType.Error);
            exitCode = -1;
            goto TheEndIsNigh;
        }
        if (!ValidateRulesFile(mo.RulesFile)) {
            WriteOutput($"InvalidCommand - RulesFile was not correct [{mo.RulesFile}]", OutputType.Error);
            exitCode = - 2;
            goto TheEndIsNigh;
        }

        var ps = new ProjectStructure {
            Root = directoryToTarget
        };
        ps.PopulateProjectStructure();
      
        var mrf = new MollyRuleFactory();
        var m = new Molly(mo);
        m.AddProjectStructure(ps);
        m.ImportRules(mrf.LoadRulesFromFile(mo.RulesFile));

        CheckResult cr;
        try {
            cr = m.ExecuteAllChecks();
            exitCode = cr.DefectCount;
        } catch (Exception ex) {
            WriteOutput(ex.ToString(),OutputType.Error);
            exitCode = - 3;
            goto TheEndIsNigh;
        }

        foreach (var l in cr.ViolationsFound) {
            WriteOutput($"{l.RuleName} ({l.Additional})",OutputType.Violation);
        }

        WriteOutput($"Total Violations {cr.DefectCount}",OutputType.Info);

    TheEndIsNigh:  // Who doesnt love a good goto, secretly.
        b.Verbose.Log("Mollycoddle, Exit");
        b.Flush();
        return exitCode;
    }

    

    private static void WriteOutputDefault(string v, OutputType ot) {
        string pfx = "";
        switch (ot) {
            case OutputType.Violation: pfx = "Violation"; break;
            case OutputType.Error: pfx = "Error"; break;
            case OutputType.Info: pfx = "Info"; break;
        }
        Console.WriteLine($"{pfx} - {v}");
    }

    private static void WriteOutputAzDo(string v, OutputType ot) {
        string pfx = "";
        switch (ot) {
            case OutputType.Violation: pfx = "##[warning] Violation"; break;
            case OutputType.Error: pfx = "##[error] Error"; break;
            case OutputType.Info: pfx = "##[command]"; break;
        }
        Console.WriteLine($"{pfx} - {v}");
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