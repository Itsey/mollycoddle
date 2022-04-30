// See https://aka.ms/new-console-template for more information

using mollycoddle;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;

internal class Program {
    static int Main(string[] args) {
        Bilge.SetConfigurationResolver( (x, y) => {
            return System.Diagnostics.SourceLevels.Verbose;
        });
        Bilge b = new Bilge("mollycoddle");
        b.AddHandler(new TCPHandler("127.0.0.1", 9060, true));

        Bilge.Alert.Online("mollycoddle");

        string masterFolder = @"C:\Files\OneDrive\Dev\Templates";


        string directoryToTarget;
        if (args.Length == 0) {
            Console.WriteLine("You need to pass a directory");
            return -1;            
        } else {
            directoryToTarget = args[0];
        }
        b.Verbose.Log($"targetting {directoryToTarget}");
        ValidateDirectory(directoryToTarget);


        var ps = new ProjectStructure();
        ps.Root = directoryToTarget;
        ps.PopulateProjectStructure();

        var dst = new DirectoryStructureChecker(ps);

        var mrf = new MollyRuleFactory();

        foreach (var x in mrf.GetRules()) {
            b.Info.Log($"Rule {x.Name} loaded");

            foreach (var n in x.Validators) {
                dst.AddRuleRequirement(n);
            }
        }

        var cr = dst.CheckDirectories();


        foreach (var l in cr.ViolationsFound) {
            Console.WriteLine($"Violation {l.RuleName} ({l.Additional})");
        }

        Console.WriteLine($"Total Violations {cr.DefectCount}");
        //dst.CheckDirectories(directoryToTarget, dirs);

        b.Flush();
        return cr.DefectCount;
    }
    static void ValidateDirectory(string pathToCheck) {
        if (!Directory.Exists(pathToCheck)) {
            throw new FileNotFoundException(pathToCheck);
        }
    }

}