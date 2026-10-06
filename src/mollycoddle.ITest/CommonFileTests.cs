using Shouldly;
using Xunit;

namespace mollycoddle.ITest;

public class CommonFileTests {
    [Fact]
    public async Task Fix_creates_missing_target_directories_and_a_second_scan_passes() {
        using var workspace = new TestWorkspace();
        workspace.WritePrimary("common.editorconfig", "expected contents");
        string rules = workspace.WriteRules(new TestRule(
            "IT_COMMON", "MatchWithPrimary", @"%ROOT%\src\nested\.editorconfig",
            AdditionalData: [@"%COMMONROOT%\common.editorconfig"]));

        var initial = await MollycoddleTestHelper.ScanAsync(
            workspace, rules, $"-primaryRoot={workspace.PrimaryDirectory}", "-fix");

        initial.AssertScan(1, "IT_COMMON");
        initial.StdOut.ShouldContain("Fix applied for supported violations", Case.Sensitive, initial.Diagnostics);
        File.ReadAllText(Path.Combine(workspace.Repository, @"src\nested\.editorconfig"))
            .ShouldBe("expected contents");

        var rerun = await MollycoddleTestHelper.ScanAsync(
            workspace, rules, $"-primaryRoot={workspace.PrimaryDirectory}");

        rerun.AssertScan(0);
    }

    [Fact]
    public async Task Failed_fix_reports_an_extra_defect_and_preserves_the_target() {
        using var workspace = new TestWorkspace();
        workspace.WritePrimary("common.editorconfig", "expected contents");
        string target = workspace.WriteFile(@"src\.editorconfig", "unchanged contents");
        string rules = workspace.WriteRules(new TestRule(
            "IT_COMMON", "MatchWithPrimary", @"%ROOT%\src\.editorconfig",
            AdditionalData: [@"%COMMONROOT%\common.editorconfig"]));
        File.SetAttributes(target, FileAttributes.ReadOnly);
        try {
            var result = await MollycoddleTestHelper.ScanAsync(
                workspace, rules, $"-primaryRoot={workspace.PrimaryDirectory}", "-fix");

            result.AssertScan(2, "IT_COMMON", "CommonFilesFetchError");
            result.StdOut.ShouldContain("Failed to fetch common files", Case.Sensitive, result.Diagnostics);
            result.StdOut.ShouldNotContain("Fix applied for supported violations", Case.Sensitive, result.Diagnostics);
            File.ReadAllText(target).ShouldBe("unchanged contents");
        } finally {
            File.SetAttributes(target, FileAttributes.Normal);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Normal_scan_compares_selected_files_without_modifying_the_repository(bool matches) {
        using var workspace = new TestWorkspace();
        workspace.WritePrimary("common.editorconfig", "expected contents");
        string target = workspace.WriteFile(@"src\.editorconfig",
            matches ? "expected contents" : "different contents");
        workspace.WriteFile(@"other\.editorconfig", "unselected contents");
        string rules = workspace.WriteRules(new TestRule(
            "IT_COMMON", "MatchWithPrimary", @"%ROOT%\src\.editorconfig",
            AdditionalData: [@"%COMMONROOT%\common.editorconfig"]));
        string[] before = Snapshot(workspace.Repository);

        var result = await MollycoddleTestHelper.ScanAsync(
            workspace, rules, $"-primaryRoot={workspace.PrimaryDirectory}");

        result.AssertScan(matches ? 0 : 1, matches ? [] : ["IT_COMMON"]);
        Snapshot(workspace.Repository).ShouldBe(before);
        if (!matches) {
            result.StdOut.ShouldContain(target.ToLowerInvariant(), Case.Sensitive, result.Diagnostics);
        }
    }

    [Fact]
    public async Task Fix_replaces_only_the_mapped_file_and_a_second_scan_passes() {
        using var workspace = new TestWorkspace();
        workspace.WritePrimary("common.editorconfig", "expected contents");
        string target = workspace.WriteFile(@"src\.editorconfig", "outdated contents");
        string unrelated = workspace.WriteFile(@"other\.editorconfig", "unrelated contents");
        string rules = workspace.WriteRules(new TestRule(
            "IT_COMMON", "MatchWithPrimary", @"%ROOT%\src\.editorconfig",
            AdditionalData: [@"%COMMONROOT%\common.editorconfig"]));

        var initial = await MollycoddleTestHelper.ScanAsync(
            workspace, rules, $"-primaryRoot={workspace.PrimaryDirectory}", "-fix");

        initial.AssertScan(1, "IT_COMMON");
        initial.StdOut.ShouldContain("Fix applied for supported violations", Case.Sensitive, initial.Diagnostics);
        File.ReadAllText(target).ShouldBe("expected contents");
        File.ReadAllText(unrelated).ShouldBe("unrelated contents");

        var rerun = await MollycoddleTestHelper.ScanAsync(
            workspace, rules, $"-primaryRoot={workspace.PrimaryDirectory}");

        rerun.AssertScan(0);
    }

    [Fact]
    public async Task Missing_primary_file_reports_an_execution_error() {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(@"src\.editorconfig");
        string rules = workspace.WriteRules(new TestRule(
            "IT_COMMON", "MatchWithPrimary", @"%ROOT%\src\.editorconfig",
            AdditionalData: [@"%COMMONROOT%\missing.editorconfig"]));

        var result = await MollycoddleTestHelper.ScanAsync(
            workspace, rules, $"-primaryRoot={workspace.PrimaryDirectory}");

        result.ExitCode.ShouldBe(-3, result.Diagnostics);
        result.StdOut.ShouldContain("Unable to find primary file", Case.Sensitive, result.Diagnostics);
        result.StdOut.ShouldNotContain("No Violations, Mollycoddle Pass.", Case.Sensitive, result.Diagnostics);
        result.StdErr.ShouldBeEmpty(result.Diagnostics);
    }

    private static string[] Snapshot(string root) =>
        [.. Directory.GetFiles(root, "*", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .Select(path => $"{Path.GetRelativePath(root, path)}:{Convert.ToHexString(File.ReadAllBytes(path))}")];
}
