using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.PowerShell;
using Nuke.Common.Utilities.Collections;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Serilog;

public partial class Build : NukeBuild {
    public Bilge b = new("Nuke", tl: System.Diagnostics.SourceLevels.Verbose);

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository]
    private readonly GitRepository GitRepository;

    [Solution]
    private readonly Solution Solution;

    [Parameter("Specifies a quick version command for the versioning quick step", Name = "QuickVersion")]
    readonly string QuickVersion = "";

    [Parameter("PreRelease will only release a pre-release verison of the package.  Uses pre-release versioning.")]
    readonly bool PreRelease = true;

    [Parameter("Full version number")]
    private string FullVersionNumber = string.Empty;

    private AbsolutePath ArtifactsDirectory = Path.Combine(Path.GetTempPath(), "_build\\mcbld\\");

    private LocalBuildConfig settings;


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
    public Target NexusLive => _ => _
      .After(Initialise)
      .DependsOn(Initialise)
      .Executes(() => {
          string? dotb = Environment.GetEnvironmentVariable("DOTB_BUILDTOOLS");
          if (!string.IsNullOrWhiteSpace(dotb)) {
              Log.Information($"Build> Ensure Nexus Is Live>  Build Tools Directory: {dotb}");

              string nexusInitScript = Path.Combine(dotb, "scripts", "nexusInit.ps1");
              if (File.Exists(nexusInitScript)) {
                  PowerShellTasks.PowerShell(x =>
                     x.SetFile(nexusInitScript)
                     .SetFileArguments("checkup")
                     .SetProcessToolPath("pwsh")
                  );
              } else {
                  Log.Error($"Build>Initialise>  Build Tools Directory: {nexusInitScript} - Nexus Init Script not found.");
              }

          } else {
              Log.Information("Build>Initialise>  Build Tools Directory: Not Set, no additional initialisation taking place.");
          }
      });

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

              Bilge.Alert.Online("Mollycoddle-Build");
              b.Info.Log("Mollycoddle Build Process Initialised, preparing Initialisation section.");

              settings = new LocalBuildConfig {
                  ExecutingMachineName = Environment.MachineName,
                  NonDestructive = false,
                  MainProjectName = "Mollycoddle",
                  MollyPrimaryToken = "%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/primaryfiles/XXVERSIONNAMEXX/",
                  MollyRulesToken = "%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/molly/XXVERSIONNAMEXX/defaultrules.mollyset",
                  MollyRulesVersion = "default",
                  ArtifactsDirectory = ArtifactsDirectory,
                  DependenciesDirectory = Solution.Projects.First(x => x.Name == "_Dependencies").Directory,
                  VersioningPersistanceTokenPre = @"%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/vstore/molly-pre.vstore",
                  VersioningPersistanceTokenRelease = @"%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/vstore/molly.vstore"
              };


              string configPath = Path.Combine(settings.DependenciesDirectory, "configuration\\");

              if (settings.NonDestructive) {
                  Log.Information("Build>Initialise>  Finish - In Non Destructive Mode.");
              } else {
                  Log.Information("Build>Initialise> Finish - In Destructive Mode.");
              }
          });
}