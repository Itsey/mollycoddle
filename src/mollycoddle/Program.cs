// See https://aka.ms/new-console-template for more information

using mollycoddle;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Plumbing;

internal class Program {
    static int Main(string[] args) {
        
        var clas = new CommandArgumentSupport();
        clas.ArgumentPrefix = "-";
        clas.ArgumentPostfix = "=";
        var mo = clas.ProcessArguments<MollyCommandLine>(args).GetOptions();

        Bilge.SetConfigurationResolver( (x, y) => {
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

        var dst = new DirectoryStructureChecker(ps,mo);
        var fst = new FileStructureChecker(ps,mo);

        var mrf = new MollyRuleFactory();

        foreach (var x in mrf.GetRules()) {
            b.Info.Log($"Rule {x.Name} loaded");

            foreach (var n in x.Validators) {
                dst.AddRuleRequirement(n);
                fst.AddRuleRequirement(n);
            }
        }

        var cr = dst.Check();
        cr = fst.Check(cr);

        foreach (var l in cr.ViolationsFound) {
            Console.WriteLine($"Violation {l.RuleName} ({l.Additional})");
        }

        Console.WriteLine($"Total Violations {cr.DefectCount}");
        
        b.Flush();
        return cr.DefectCount;
    }
    static void ValidateDirectory(string pathToCheck) {
        if (!Directory.Exists(pathToCheck)) {
            throw new FileNotFoundException(pathToCheck);
        }
    }

}