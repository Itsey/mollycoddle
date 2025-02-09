using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {

    // ArrangeStep = Well Known Initial Step for correctness and Linting. [Arrange] Construct Examine Package Release Test
    public Target ArrangeStep => _ => _
        .Before(ConstructStep, Wrapup)
        .DependsOn(Initialise)
        .Triggers(Clean, MollyCheck)
        .Executes(() => {
            Log.Information("--> Arrange <-- ");
        });


    private Target Clean => _ => _
        .DependsOn(Initialise)
        .After(ArrangeStep, Initialise)
        .Before(ConstructStep)
        .Executes(() => {
            b.Info.Log("Clean Step in Arrange Starts");

            DotNetTasks.DotNetClean(s => s.SetProject(Solution));

            b.Verbose.Log("Clean completed, cleaning artefact directory");

            settings.ArtifactsDirectory.CreateOrCleanDirectory();
        });





    private Target MollyCheck => _ => _
       .After(Clean, ArrangeStep)
       .Before(ConstructStep)
       .Executes(() => {
           Log.Information("Mollycoddle Structure Linting Starts.");

           string mollyEnabledMachihnes = "UNICORN,DAVYJONESx";
           bool molsActive = false;
           string thisMachine = System.Environment.MachineName.ToUpperInvariant();
           foreach (string item in mollyEnabledMachihnes.Split(",")) {
               if (item.ToUpperInvariant() == thisMachine) {
                   b.Verbose.Log($"Molly Active - {thisMachine}");
                   molsActive = true;
                   break;
               }
           }

           //TODO: Bug LFY-8. https://plisky.atlassian.net/browse/LFY-8
           if (!molsActive) {
               Log.Information("Mollycoddle Structure Linting Skipped - Machine Wide Disablement.");
               return;
           }

           var mc = new MollycoddleTasks();
           mc.PerformScan(s => s
               .AddRuleHelp(true)
               .SetRulesFile(@"C:\files\code\git\mollycoddle\src\_Dependencies\RulesFiles\XXVERSIONNAMEXX\defaultrules.mollyset")
          .SetPrimaryRoot(@"C:\Files\OneDrive\Dev\PrimaryFiles")
          .SetDirectory(GitRepository.LocalDirectory));

           Log.Information("Mollycoddle Structure Linting Completes.");

       });
}