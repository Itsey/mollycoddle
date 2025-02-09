using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities.Collections;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Serilog;

partial class Build : NukeBuild {
    public Bilge b = new("Nuke", tl: System.Diagnostics.SourceLevels.Verbose);


    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository]
    readonly GitRepository GitRepository;

    [Solution]
    readonly Solution Solution;

    AbsolutePath ArtifactsDirectory => @"D:\Scratch\_build\mcbld\";


    private LocalBuildConfig settings;

    protected const string VersionStorePath = @"D:\Scratch\_build\vstore\mollycoddle-version.vstore";


    public Target Wrapup => _ => _
        .DependsOn(Initialise)
        .After(Initialise)
        .Executes(() => {
            b.Info.Log("Build >> Wrapup >> All Done.");
            Log.Information("Build>Wrapup>  Finish - Build Process Completed.");
            b.Flush().Wait();
            System.Threading.Thread.Sleep(10);
        });

    protected override void OnBuildFinished() {

        string lb = !Build.IsLocalBuild ? $"Server [{settings.ExecutingMachineName}]" : $"Local [{settings.ExecutingMachineName}]";

        string wrked = string.Empty;
        if (IsSucceeding) {
            wrked = "Succeeded";

        } else {
            wrked = "Failed (";
            FailedTargets.ForEach(x => {
                wrked += x.Name + ", ";
            });
            wrked += ")";
        }
        Log.Information($"Build>Wrapup>  {wrked}.");


    }

    public Target Initialise => _ => _
          .Before(ExamineStep, Wrapup)
          .Triggers(Wrapup)
          .Executes(() => {
              if (Solution == null) {
                  Log.Error("Build>Initialise>Solution is null.");
                  throw new InvalidOperationException("The solution must be set");
              }

              Bilge.AddHandler(new TCPHandler("127.0.0.1", 9060, true));

              Bilge.SetConfigurationResolver((a, b) => {
                  return System.Diagnostics.SourceLevels.Verbose;
              });

              b = new Bilge("Nuke", tl: System.Diagnostics.SourceLevels.Verbose);


              Bilge.Alert.Online("Listify-Build");
              b.Info.Log("Listify Build Process Initialised, preparing Initialisation section.");


              settings = new LocalBuildConfig();
              settings.ExecutingMachineName = Environment.MachineName;
              settings.NonDestructive = false;
              settings.MainProjectName = "Listify";
              settings.ArtifactsDirectory = ArtifactsDirectory;
              settings.DependenciesDirectory = Solution.Projects.First(x => x.Name == "_Dependencies").Directory;

              string configPath = Path.Combine(settings.DependenciesDirectory, "configuration\\");

              if (settings.NonDestructive) {
                  Log.Information("Build>Initialise>  Finish - In Non Destructive Mode.");
              } else {
                  Log.Information("Build>Initialise> Finish - In Destructive Mode.");
              }


          });


}
