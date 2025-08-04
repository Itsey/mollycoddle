using System;
using Nuke.Common;
using Nuke.Common.Tools.NuGet;
using Serilog;

public partial class Build : NukeBuild {

    // Well known step for releasing into the selected environment.  Arrange Construct Examine Package [Release] Test
    public Target ReleaseStep => _ => _
      .DependsOn(Initialise, PackageStep)
      .Before(TestStep, Wrapup)
      .After(PackageStep)
      .Executes(() => {
          if ((!IsLocalBuild) && (Configuration != Configuration.Release)) {
              Log.Error("ReleaseStep is only valid in Release mode");
              throw new InvalidOperationException("DeployStep is only valid in Release mode for remote builds.");
          }

          Log.Information("--> Release Step - Releasing Nuget Package <-- ");
          NuGetTasks.NuGetPush(s => s
            .SetTargetPath(ArtifactsDirectory + "\\Plisky.Mollycoddle*.nupkg")
            .SetSource("https://api.nuget.org/v3/index.json")
            .SetApiKey(Environment.GetEnvironmentVariable("PLISKY_PUBLISH_KEY")));
      });
}