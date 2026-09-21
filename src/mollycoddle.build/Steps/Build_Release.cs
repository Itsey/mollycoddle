using System;
using System.Linq;
using Fallout.Common;
using Fallout.Common.Tools.Git;
using Fallout.Common.Tools.NuGet;
using Serilog;

public partial class Build : FalloutBuild {

    // Well known step for releasing into the selected environment.  Arrange Construct Examine Package [Release] Test
    public Target ReleaseStep => _ => _
      .DependsOn(Initialise, PackageStep)
      .Before(TestStep, Wrapup)
      .After(PackageStep)
      .Triggers(ApplyGitTag)
      .Executes(() => {
          if ((!IsLocalBuild) && (Configuration != Configuration.Release)) {
              Log.Error("ReleaseStep is only valid in Release mode");
              throw new InvalidOperationException("DeployStep is only valid in Release mode for remote builds.");
          }

          Log.Information("--> Release Step - Releasing Nuget Package <-- ");
          NuGetTasks.NuGetPush(s => s
            .SetTargetPath(ArtifactsDirectory + "\\nuget\\Plisky.Mollycoddle*.nupkg")
            .SetSource("https://api.nuget.org/v3/index.json")
            .SetApiKey(Environment.GetEnvironmentVariable("PLISKY_PUBLISH_KEY")));
      });

    public Target ApplyGitTag => _ => _
      .After(ReleaseStep)
      .DependsOn(Initialise)
      .Before(Wrapup)
      .Executes(() => {
          if (!IsSucceeding) {
              return;
          }

          if (settings.NonDestructive) {
              Log.Information("Non destructive, skipping Tag");
              return;
          }

          if (string.IsNullOrEmpty(FullVersionNumber)) {
              Log.Information("No version number, skipping Tag");
              return;
          }

          if (GitRepository == null) {
              Log.Information("No Git repository found, skipping Tag");
              return;
          }

          Log.Information("Applying Git Tag");

          try {
              var (authorName, authorEmail) = GetLastCommitAuthor();
              ConfigureGitIdentity(authorName, authorEmail);
              CreateAndPushTag(FullVersionNumber);

              Log.Information($"Successfully created and pushed tag '{FullVersionNumber}'");
          } catch (Exception ex) {
              Log.Error($"Failed to apply git tag: {ex.Message}");
              throw;
          }
      });

    private void ConfigureGitIdentity(string name, string email) {
        GitTasks.Git($"config user.name \"{name}\"");
        GitTasks.Git($"config user.email \"{email}\"");
    }

    private void CreateAndPushTag(string version) {
        GitTasks.Git($"tag -a {version} -m \"Release {version}\"");
        GitTasks.Git($"push origin {version}");
    }

    private (string Name, string Email) GetLastCommitAuthor() {
        string name = GitTasks.Git("log -1 --pretty=format:%an").First().Text;
        string email = GitTasks.Git("log -1 --pretty=format:%ae").First().Text;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email)) {
            throw new InvalidOperationException("Unable to retrieve commit author information");
        }

        return (name, email);
    }
}