using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {

    // Standard entrypoint for compiling the app.  Arrange [Construct] Examine Package Release Test

    public Target ConstructStep => _ => _
        .Before(ExamineStep, Wrapup)
        .After(ArrangeStep)
        .Triggers(Compile)
        .DependsOn(Initialise, ArrangeStep)
        .Executes(() => {
        });


    private Target Restore => _ => _
       .After(ConstructStep)
       .Before(Compile)
       .Executes(() => {
       });


    public Target VersionSource => _ => _.Executes(() => {

        /*  if (IsLocalBuild) {
              Log.Information("Local build, skipping versioning");
              return;
          }*/

        Log.Information($"Versioning {VersionStorePath}");

        var vf = new VersonifyTasks();
        vf.PassiveExecute(s => s
          .SetRoot(Solution.Directory)
          .SetVersionPersistanceValue(VersionStorePath)
          .SetDebug(false));

        vf.PerformFileUpdate(s => s
         .SetRoot(Solution.Directory)
         .AddMultimatchFile($"{Solution.Directory}\\_Dependencies\\Automation\\AutoVersion.txt")
         .PerformIncrement(true)
         .SetVersionPersistanceValue(VersionStorePath)
         .SetDebug(false)
         .SetRelease(""));

        //Logger.Info($"Version is {version}");
    });


    private Target Compile => _ => _
        .Before(ExamineStep)
        .DependsOn(Restore, VersionSource)
        .Executes(() => {
            DotNetTasks.DotNetBuild(s => s
              .SetProjectFile(Solution)
              .SetConfiguration(Configuration)
              .SetDeterministic(IsServerBuild)
              .SetContinuousIntegrationBuild(IsServerBuild)
          );
        });
}