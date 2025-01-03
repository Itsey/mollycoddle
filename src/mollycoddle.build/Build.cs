using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using Plisky.Nuke.Fusion;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

class Build : NukeBuild {


    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository]
    readonly GitRepository GitRepository;

    [Solution]
    readonly Solution Solution;

    AbsolutePath ArtifactsDirectory => @"D:\Scratch\_build\mcbld\";

    protected const string VersionStorePath = @"D:\Scratch\_build\vstore\mollycoddle-version.vstore";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() => {

            DotNetTasks.DotNetClean(s => s
             .SetProject(Solution));

            ArtifactsDirectory.CreateOrCleanDirectory();

        });

    Target PrecompileScan => _ => _
    .Before(Clean)
    .Executes(() => {

        MollycoddleTasks.PerformScan(s => s
          .AddRuleHelp(true)
          .SetRulesFile(@"C:\files\code\git\mollycoddle\src\_Dependencies\RulesFiles\XXVERSIONNAMEXX\defaultrules.mollyset")
          .SetPrimaryRoot(@"C:\Files\OneDrive\Dev\PrimaryFiles")
          .SetDirectory(GitRepository.LocalDirectory)
        );
        

        ArtifactsDirectory.CreateOrCleanDirectory();

    });


    Target Restore => _ => _
        .Executes(() => {
        });


    Target VersionSource => _ => _
    .Executes(() => {

        if (IsLocalBuild) {
            Logger.Info("Local build, skipping versioning");
            return;
        }


        VersonifyTasks.PassiveExecute(s => s
          .SetRoot(Solution.Directory)
          .SetVersionPersistanceValue(VersionStorePath)
          .SetDebug(false));

        VersonifyTasks.PerformFileUpdate(s => s
         .SetRoot(Solution.Directory)
         .AddMultimatchFile($"{Solution.Directory}\\_Dependencies\\Automation\\AutoVersion.txt")
         .PerformIncrement(true)
         .SetVersionPersistanceValue(VersionStorePath)
         .SetDebug(false)
         .AsDryRun(true)
         .SetRelease(""));

        //Logger.Info($"Version is {version}");
    });


    Target UnitTest => _ => _
        .After(Compile)
        .Executes(() => {
            var testProjects = Solution.GetAllProjects("*.test");
            if (testProjects.Any()) {
                DotNetTasks.DotNetTest(s => s
                    .EnableNoRestore()
                    .EnableNoBuild()
                    .SetProjectFile(testProjects.First().Directory));
            } else {
                Logger.Info("No test projects found");
            }
        });

    Target Compile => _ => _
        .Triggers(Publish, UnitTest)
        .DependsOn(VersionSource, Clean)
        .DependsOn(Restore)
        .Executes(() => {

            DotNetTasks.DotNetBuild(s => s
               .SetProjectFile(Solution)
               .SetConfiguration(Configuration)
               .EnableNoRestore()
               .SetDeterministic(IsServerBuild)
               .SetContinuousIntegrationBuild(IsServerBuild)
           );

        });


    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() => {
            var project = Solution.GetProject("mollycoddle");

            if (project == null) { throw new InvalidOperationException("Project not found"); }

            var publishDirectory = ArtifactsDirectory + "\\publish\\mollycoddle";
            var nugetStructure = ArtifactsDirectory + "\\nuget";
            var nugetTools = nugetStructure + "\\tools";

            DotNetTasks.DotNetPublish(s => s
              .SetProject(project)
              .SetConfiguration(Configuration)
              .SetOutput(publishDirectory)
              .EnableNoRestore()
              .EnableNoBuild()
            );

            nugetTools.DeleteDirectory();
            FileSystemTasks.CopyDirectoryRecursively(publishDirectory, nugetTools, DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);

            string readmeFile = Solution.GetProject("_Dependencies").Directory + "\\packaging\\readme.md";
            string targetdir = nugetStructure + "\\readme.md";
            FileSystemTasks.CopyFile(readmeFile, targetdir, FileExistsPolicy.Overwrite);

            string nugetPackageFile = Solution.GetProject("_Dependencies").Directory + "\\packaging\\mollycoddle.nuspec";
            FileSystemTasks.CopyFile(nugetPackageFile, ArtifactsDirectory + "\\mollycoddle.nuspec", FileExistsPolicy.Overwrite);

        });

    Target BuildNugetPackage => _ => _
      .DependsOn(Publish)
      .After(Compile, Publish)
      .Executes(() => {
          VersonifyTasks.CommandPassive(s => s
            .SetRoot(Solution.Directory)
            .SetVersionPersistanceValue(VersionStorePath)
            .SetDebug(true));

          VersonifyTasks.IncrementAndUpdateFiles(s => s
           .SetRoot(ArtifactsDirectory)
           .AddMultimatchFile($"{Solution.Directory}\\_Dependencies\\Automation\\NuspecVersion.txt")
           .SetVersionPersistanceValue(@"c:\temp\vs.store")
           .SetDebug(true)
           .AsDryRun(true)
           .SetRelease(""));

          NuGetTasks.NuGetPack(s => s
            .SetTargetPath(ArtifactsDirectory + "\\mollycoddle.nuspec")
            .SetOutputDirectory(ArtifactsDirectory));



      });

    Target ReleaseNugetPackage => _ => _
     .DependsOn(BuildNugetPackage)
     .After(UnitTest, Compile, Publish, BuildNugetPackage)
     .Executes(() => {
         NuGetTasks.NuGetPush(s => s
             .SetTargetPath(ArtifactsDirectory + "\\Plisky.Mollycoddle*.nupkg")
             .SetSource("https://api.nuget.org/v3/index.json")
             .SetApiKey(Environment.GetEnvironmentVariable("PLISKY_PUBLISH_KEY")));
     });
}
