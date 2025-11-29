using System;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {
    // Standard entrypoint for compiling the app.  Arrange [Construct] Examine Package Release Test

    public Target ApplyVersion => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)

      .Requires(() => Solution != null)
      .Before(Compile)
      .Executes(() => {
          if (settings == null) {
              Log.Error("Build>ApplyVersion>Settings is null.");
              throw new InvalidOperationException("The settings must be set");
          }

          bool dryRunMode = IsLocalBuild;

          string versioningType = "Pre-Release";
          string vtFile = settings.VersioningPersistanceTokenPre;
          if (!PreRelease) {
              vtFile = settings.VersioningPersistanceTokenRelease;
              versioningType = "Release";
          }

          Log.Information($"[Versioning] {versioningType} versioning starts. DryRun = {dryRunMode}");

          var vc = new VersonifyTasks();

          var mmPathBase = settings.DependenciesDirectory / "automation";
          var mmPath = mmPathBase / "autoversion.txt";

          vc.FileUpdateCommand(s => s
              .SetVersionPersistanceValue(vtFile)
              .AddMultimatchFile(mmPath)
              .PerformIncrement(true)
              .SetOutputStyle("console-nf")
              .AsDryRun(dryRunMode)
              .SetRoot(Solution.Directory)
          );

          Log.Information($"[Versioning] {versioningType} Incremented, updated files.({vc.VersionLiteral})");
          FullVersionNumber = vc.VersionLiteral;

          if (!PreRelease) {
              UpdatePreReleaseVersionNumber(dryRunMode, versioningType, vc, mmPathBase);
          }
      });

    public Target ConstructStep => _ => _
            .Before(ExamineStep, Wrapup)
        .After(ArrangeStep)
        .Triggers(Compile, ApplyVersion)
        .DependsOn(Initialise, ArrangeStep)
        .Executes(() => {
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

    private Target Restore => _ => _
                   .After(ConstructStep)
       .Before(Compile)
       .Executes(() => {
       });

    private void UpdatePreReleaseVersionNumber(bool dryRunMode, string versioningType, VersonifyTasks vc, Nuke.Common.IO.AbsolutePath mmPathBase) {
        Log.Information($"[Versioning] {versioningType} Applying release version number to pre-release data. ({vc.VersionLiteral}) ");

        // Once the release version changes we need to update the pre-release version numbers to match the released version otherwise it will still
        // think its on the old version base.  The file update here is set to dummy files just so that the version store is updated. This will catch
        // an edge case where two release versions occur.

        vc.OverrideCommand(s => s
            .SetVersionPersistanceValue(settings.VersioningPersistanceTokenPre)
            .SetOutputStyle("console-nf")
            .AsDryRun(dryRunMode)
            .SetRoot(Solution.Directory)
            .SetQuickValue("..+")
        );

        var nmPath = mmPathBase / "noversion.txt";
        vc.FileUpdateCommand(s => s
            .SetVersionPersistanceValue(settings.VersioningPersistanceTokenPre)
            .SetOutputStyle("console-nf")
            .AddMultimatchFile(nmPath)
            .PerformIncrement(true)
            .AsDryRun(dryRunMode)
            .SetRoot(Solution.Directory / "_Dependencies" / "Utils")
        );
    }
}