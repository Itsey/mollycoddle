using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

public partial class Build : NukeBuild {


    // TestStep is the well known post release integration test step. Arrange Construct Examine Package Release [Test]
    public Target TestStep => _ => _
        .After(ConstructStep)
        .Before(Wrapup)
        .DependsOn(Initialise)
        .Triggers(IntegrationTest)
        .Executes(() => {
            Log.Information("--> Examine Step <-- ");
        });

    private Target IntegrationTest => _ => _
      .Executes(() => {
          var testProjects = Solution.GetAllProjects("*.ITest");
          if (testProjects.Any()) {
              DotNetTasks.DotNetTest(s => s
                  .EnableNoRestore()
                  //.EnableNoBuild()
                  .SetConfiguration(Configuration)
                  .SetProjectFile(testProjects.First().Directory));
          }
      });
}