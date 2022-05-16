// See https://aka.ms/new-console-template for more information

using mollycoddle;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Plumbing;

internal class Program {

    private static int Main(string[] args) {
        var clas = new CommandArgumentSupport();
        clas.ArgumentPrefix = "-";
        clas.ArgumentPostfix = "=";
        var ma = clas.ProcessArguments<MollyCommandLine>(args);
        if (ma.Disabled) {
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

        string directoryToTarget = mo.DirectoryToTarget;

        b.Verbose.Log($"targetting {directoryToTarget}");
        ValidateDirectory(directoryToTarget);

        var ps = new ProjectStructure();
        ps.Root = directoryToTarget;
        ps.PopulateProjectStructure();
      
        var mrf = new MollyRuleFactory();
        var m = new Molly(mo);
        m.AddProjectStructure(ps);
        m.ImportRules(mrf.LoadRulesFromFile(mo.RulesFile));

        var cr = m.ExecuteAllChecks();
       
        foreach (var l in cr.ViolationsFound) {
            Console.WriteLine($"Violation {l.RuleName} ({l.Additional})");
        }

        Console.WriteLine($"Total Violations {cr.DefectCount}");

        b.Flush();
        return cr.DefectCount;
    }

    private static void ValidateDirectory(string pathToCheck) {
        if (!Directory.Exists(pathToCheck)) {
            throw new FileNotFoundException(pathToCheck);
        }
    }
}