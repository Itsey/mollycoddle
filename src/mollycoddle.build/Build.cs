using System;
using System.IO;
using System.Linq;
using Fallout.Common;
using Fallout.Common.Git;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.PowerShell;
using Fallout.Common.Utilities.Collections;
using Fallout.Solutions;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Serilog;

public partial class Build : FalloutBuild {
    public Bilge b = new("Fallout", tl: System.Diagnostics.SourceLevels.Verbose);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository]
    private readonly GitRepository GitRepository;

    [Parameter("PreRelease will only release a pre-release verison of the package.  Uses pre-release versioning.")]
    readonly bool PreRelease = true;

    [Parameter("Specifies a quick version command for the versioning quick step", Name = "QuickVersion")]
    readonly string QuickVersion = "";

    [Solution]
    private readonly Solution Solution;

    private readonly AbsolutePath ArtifactsDirectory = Path.Combine(Path.GetTempPath(), "_build\\mcbld\\");

    [Parameter("Full version number")]
    private string FullVersionNumber = string.Empty;

    private LocalBuildConfig settings;

    public Target Initialise => _ => _
          .Before(ExamineStep, Wrapup)
          .Triggers(Wrapup)
          .Executes(() => {
              if (Solution == null) {
                  Log.Error("Build>Initialise>Solution is null.");
                  throw new InvalidOperationException("The solution must be set");
              }

              var hnd = new TCPHandler("127.0.0.1", 9060, true);
              hnd.SetFormatter(new FlimFlamV4Formatter());
              Bilge.AddHandler(hnd);

              Bilge.SetConfigurationResolver((a, b) => {
                  return System.Diagnostics.SourceLevels.Verbose;
              });

              b = new Bilge("Fallout", tl: System.Diagnostics.SourceLevels.Verbose);

              Bilge.Alert.Online("Mollycoddle-Build");
              b.Info.Log("Mollycoddle Build Process Initialised, preparing Initialisation section.");

              settings = new LocalBuildConfig {
                  ExecutingMachineName = Environment.MachineName,
                  NonDestructive = false,
                  MainProjectName = "Mollycoddle",
                  MollyPrimaryToken = "%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/primaryfiles/XXVERSIONNAMEXX/",
                  MollyRulesToken = "%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/molly/XXVERSIONNAMEXX/defaultrules.mollyset",
                  VersioningPersistanceTokenPre = @"%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/vstore/molly-pre.vstore",
                  VersioningPersistanceTokenRelease = @"%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/vstore/molly.vstore",
                  MollyRulesVersion = "latest",
                  ArtifactsDirectory = ArtifactsDirectory,
                  DependenciesDirectory = Solution.Projects.First(x => x.Name == "_Dependencies").Directory,
              };

              string configPath = Path.Combine(settings.DependenciesDirectory, "configuration\\");

              if (settings.NonDestructive) {
                  Log.Information("Build>Initialise>  Finish - In Non Destructive Mode.");
              } else {
                  Log.Information("Build>Initialise> Finish - In Destructive Mode.");
              }
          });

    public Target NexusLive => _ => _
      .After(Initialise)
      .DependsOn(Initialise)
      .Executes(() => {
          string dotb = Environment.GetEnvironmentVariable("DOTB_BUILDTOOLS");
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

    public Target Wrapup => _ => _
        .DependsOn(Initialise)
        .After(Initialise)
        .Executes(() => {
            b.Info.Log("Build >> Wrapup >> All Done.");
            Log.Information("Build>Wrapup>  Finish - Build Process Completed.");
            b.Flush().Wait();
            System.Threading.Thread.Sleep(10);
        });

    public static int Main() => Execute<Build>(x => x.Compile);

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
        Log.Information($"Build>Wrapup>  {wrked}. Version > {FullVersionNumber}");
    }
}