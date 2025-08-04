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
        .Triggers(BuildNugetPackage)
        .Executes(() => {
            var project = Solution.GetProject("mollycoddle");
            var dependenciesDir = Solution.GetProject("_Dependencies").Directory;

            if (project == null) { throw new InvalidOperationException("Project not found"); }

            var publishDirectory = ArtifactsDirectory + "\\publish\\mollycoddle";
            var nugetStructure = ArtifactsDirectory / "nuget";
            var nugetTools = nugetStructure / "tools";
            var mcRules = nugetStructure / "rules";
            var mcQuickStart = dependenciesDir / "RulesFiles" / "QuickStart";

            DotNetTasks.DotNetPublish(s => s
              .SetProject(project)
              .SetConfiguration(Configuration)
              .SetOutput(publishDirectory)
              .EnableNoRestore()
              .SetSelfContained(false)
              .EnableNoBuild());

            nugetTools.DeleteDirectory();
            mcRules.DeleteDirectory();

            publishDirectory.CopyToDirectory(nugetTools, ExistsPolicy.FileOverwrite);
            mcQuickStart.CopyToDirectory(mcRules, ExistsPolicy.FileOverwrite);

            var readmeFile = dependenciesDir + "\\packaging\\readme.md";

            readmeFile.CopyToDirectory(nugetStructure, ExistsPolicy.FileOverwrite);
            var nugetPackageFile = Solution.GetProject("_Dependencies").Directory + "\\packaging\\mollycoddle.nuspec";
            nugetPackageFile.CopyToDirectory(ArtifactsDirectory, ExistsPolicy.FileOverwrite);
        });

    public Target BuildNugetPackage => _ => _
      .DependsOn(PackageStep)
      .After(Compile, PackageStep)
      .Before(ReleaseStep)
      .Executes(() => {
          var vf = new VersonifyTasks();

          Log.Information("Creating Nuget Package");

          vf.PassiveExecute(s => s
            .SetRoot(Solution.Directory)
            .SetVersionPersistanceValue(VersionStorePath)
            .SetDebug(true));

          vf.PerformFileUpdate(s => s
           .SetRoot(ArtifactsDirectory)
           .AddMultimatchFile($"{Solution.Directory}\\_Dependencies\\Automation\\NuspecVersion.txt")
           .SetVersionPersistanceValue(@"D:\Scratch\_build\vstore\mc-nuget-tool.vstore")
           .SetDebug(true)

           .SetRelease(""));

          NuGetTasks.NuGetPack(s => s
            .SetTargetPath(ArtifactsDirectory + "\\mollycoddle.nuspec")
            .SetOutputDirectory(ArtifactsDirectory));
      });
}