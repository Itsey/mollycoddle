using System.Linq;
using Fallout.Common;
using Fallout.Common.Tools.DotNet;
using Serilog;

public partial class Build : FalloutBuild {

    // Examine is the well known step for post compilation, pre package and deploy. Arrange Construct [Examine] Package Release Test
    public Target ExamineStep => _ => _
        .After(ConstructStep)
        .Before(PackageStep, Wrapup)
        .DependsOn(Initialise, ConstructStep)
        .Triggers(UnitTest)
        .Executes(() => {
            Log.Information("--> Examine Step <-- ");
        });

    private Target UnitTest => _ => _
      .After(ExamineStep)
      .Before(PackageStep)
      .Executes(() => {
          var testProjects = Solution.GetAllProjects("*.test");
          if (testProjects.Any()) {
              DotNetTasks.DotNetTest(s => s
                  .EnableNoRestore()
                  .EnableNoBuild()
                  .SetConfiguration(Configuration)
                  .SetProjectFile(testProjects.First().Directory));
          }
      });
}