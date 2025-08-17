using System;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {
    // Package Step - Well known step for bundling prior to the app release.   Arrange Construct Examine [Package] Release Test

    private Target PackageStep => _ => _
        .After(ExamineStep)
        .Before(ReleaseStep, Wrapup)
        .DependsOn(Initialise, ExamineStep)
        .Executes(() => {


            if (Solution == null) {
                Log.Error("Build>PackageStep>Solution is null.");
                throw new InvalidOperationException("The solution must be set");
            }

            if (settings == null) {
                Log.Error("Build>PackageStep>Settings is null.");
                throw new InvalidOperationException("The settings must be set");
            }

            var project = Solution.GetProject("mollycoddle");
            if (project == null) { throw new InvalidOperationException("Project not found"); }

            var publishDirectory = settings.ArtifactsDirectory + "\\publish\\";
            var nugetStructure = settings.ArtifactsDirectory + "\\nuget";

            DotNetTasks.DotNetPack(s => s
              .SetProject(project)
              .SetConfiguration(Configuration)
              .SetOutputDirectory(nugetStructure)
              .EnableNoBuild()
              .EnableNoRestore()
            );
        });


}