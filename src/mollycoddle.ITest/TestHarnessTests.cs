using System.Diagnostics;
using Shouldly;
using Xunit;

namespace mollycoddle.ITest;

public class TestHarnessTests {
    [Fact]
    public async Task Expired_execution_timeout_reports_the_invocation_and_streams() {
        using var workspace = new TestWorkspace();

        var error = await Should.ThrowAsync<TimeoutException>(() =>
            MollycoddleTestHelper.ExecuteAsync(workspace.Repository, ["--help"], TimeSpan.Zero));

        error.Message.ShouldContain("Mollycoddle timed out.");
        error.Message.ShouldContain(workspace.Repository);
        error.Message.ShouldContain("--help");
        error.Message.ShouldContain("stdout:");
        error.Message.ShouldContain("stderr:");
        string processLine = error.Message.Split(Environment.NewLine)[0];
        int processId = int.Parse(processLine[(processLine.IndexOf("Process ID: ", StringComparison.Ordinal) + 12)..]);
        Should.Throw<ArgumentException>(() => Process.GetProcessById(processId));
    }

    [Fact]
    public async Task Completed_scan_workspace_is_removed_on_disposal() {
        string root;
        using (var workspace = new TestWorkspace()) {
            root = workspace.Root;
            workspace.WriteFile("present.cs");
            string rules = workspace.WriteRules(new TestRule("IT_REQUIRED", "MustExist", "**/*.cs"));

            var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

            result.AssertScan(0);
            Directory.Exists(Path.Combine(workspace.CacheDirectory, "mccache")).ShouldBeTrue();
        }

        Directory.Exists(root).ShouldBeFalse();
    }
}
