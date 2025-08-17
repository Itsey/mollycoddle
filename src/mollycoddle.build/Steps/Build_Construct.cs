using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {
    // Standard entrypoint for compiling the app.  Arrange [Construct] Examine Package Release Test

    public string FullVersionNumber { get; set; } = string.Empty;

    public Target ConstructStep => _ => _
        .Before(ExamineStep, Wrapup)
        .After(ArrangeStep)
        .Triggers(Compile, ApplyVersion)
        .DependsOn(Initialise, ArrangeStep)
        .Executes(() => {
        });

    private Target Restore => _ => _
       .After(ConstructStep)
       .Before(Compile)
       .Executes(() => {
       });


    public Target VersionQuickStep => _ => _
    .After(ConstructStep)
    .DependsOn(Initialise)
    .Before(Compile)
    .Executes(() => {
        Log.Information($"Manual Quick Step QV:{QuickVersion}");

        if (settings == null) {
            Log.Error("Build>ApplyVersion>Settings is null.");
            throw new InvalidOperationException("The settings must be set");
        }

        if (Solution == null) {
            Log.Error("Build>ApplyVersion>Solution is null.");
            throw new InvalidOperationException("The solution must be set");
        }

        if (!string.IsNullOrEmpty(QuickVersion)) {
            var vc = new VersonifyTasks();

            vc.OverrideCommand(s => s
              .SetVersionPersistanceValue(settings.VersioningPersistanceTokenPre)
              .SetDebug(true)
              .SetRoot(Solution.Directory)
              .SetQuickValue(QuickVersion)
            );
        }
    });

    public Target QueryNextVersion => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)
      .Before(Compile)
      .Executes(() => {
          if (settings == null) {
              Log.Error("Build>ApplyVersion>Settings is null.");
              throw new InvalidOperationException("The settings must be set");
          }

          if (Solution == null) {
              Log.Error("Build>ApplyVersion>Solution is null.");
              throw new InvalidOperationException("The solution must be set");
          }

          string versioningToken = settings.VersioningPersistanceTokenRelease;
          if (PreRelease) {
              Log.Information("Build>QueryNextVersion>PreRelease is set, using non release Token.");
              versioningToken = settings.VersioningPersistanceTokenPre;
          }
          var vc = new VersonifyTasks();
          vc.PassiveCommand(s => s
          .SetVersionPersistanceValue(versioningToken)
          .SetOutputStyle("con-nf")
          .SetRoot(Solution.Directory));

          Log.Information($"Version Is:{vc.VersionLiteral}");
      });

    public Target ApplyVersion => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)
      .Before(Compile)
      .Executes(() => {
          if (settings == null) {
              Log.Error("Build>ApplyVersion>Settings is null.");
              throw new InvalidOperationException("The settings must be set");
          }

          if (Solution == null) {
              Log.Error("Build>ApplyVersion>Solution is null.");
              throw new InvalidOperationException("The solution must be set");
          }

          bool dryRunMode = false;

          if (IsLocalBuild) {
              // Passive Get Current Version
              Log.Information("Local Build - Versioning Set To Dry Run");
              dryRunMode = true;
          }

          string versioningType = "Pre-Release";
          string vtFile = settings.VersioningPersistanceTokenPre;
          if (!PreRelease) {
              vtFile = settings.VersioningPersistanceTokenRelease;
              versioningType = "Release";
          }

          Log.Information($"[Versioning]{versioningType} versioning displaying curent version number.");

          var vc = new VersonifyTasks();
          vc.PassiveCommand(s => s
              .SetVersionPersistanceValue(vtFile)
              .SetOutputStyle("azdo")
              .SetRoot(Solution.Directory)
          );

          var mmPath = settings.DependenciesDirectory / "automation";
          mmPath /= "autoversion.txt";

          vc.FileUpdateCommand(s => s
              .SetVersionPersistanceValue(vtFile)
              .AddMultimatchFile(mmPath)
              .PerformIncrement(true)
              .SetOutputStyle("azdo-nf")
              .AsDryRun(dryRunMode)
              .SetRoot(Solution.Directory)
          );

          // BUG.  vc.VersionLiteral is not set to the FileUpdateCommand output so have queued another passive to fix this.

          vc.PassiveCommand(s => s
             .SetVersionPersistanceValue(vtFile)
             .SetOutputStyle("azdo")
             .SetRoot(Solution.Directory)
         );

          Log.Information($"[Versioning]{versioningType} Increment and Update Exsiting Files.({vc.VersionLiteral})");

          if (!PreRelease) {
              // Hack.  Curently the return from versonify is not set to be different display types, we need 3 digit for semver so hacking the last digit off.

              int periodCount = 0;
              string verNumberToUse = string.Empty;
              foreach (char c in vc.VersionLiteral) {
                  if (c == '.') {
                      if (periodCount == 2) {
                          break;
                      }
                      periodCount++;
                  }

                  verNumberToUse += c;
              }

              while (periodCount < 2) {
                  verNumberToUse += ".0";
                  periodCount++;
              }

              b.Assert.True(verNumberToUse.Count(x => x == '.') == 2, $"The version number ({verNumberToUse}) should be in the format N.N.N.");
              Log.Information($"[Versioning]{versioningType} Applying release version number to pre-release data. ({vc.VersionLiteral} > {verNumberToUse})");

              vc.OverrideCommand(s => s
                  .SetVersionPersistanceValue(settings.VersioningPersistanceTokenPre)
                  .SetOutputStyle("azdo-nf")
                  .AsDryRun(dryRunMode)
                  .SetRoot(Solution.Directory)
                  .SetQuickValue(verNumberToUse)
              );
          }

          FullVersionNumber = vc.VersionLiteral;
          Log.Information($"[Versioning]Version applied:{vc.VersionLiteral}");
      });

    private Target Compile => _ => _
        .Before(ExamineStep)
        .DependsOn(Restore)
        .Executes(() => {
            DotNetTasks.DotNetBuild(s => s
              .SetProjectFile(Solution)
              .SetConfiguration(Configuration)
              .SetDeterministic(IsServerBuild)
              .SetContinuousIntegrationBuild(IsServerBuild)
          );
        });
}