using Shouldly;
using Xunit;

namespace mollycoddle.ITest;

public class CommandLineTests {

    //TODO: LFY-92. This test covers the current (buggy) behaviour for regression.
    // This test should be removed or updated as part of the bug fix.
    [Fact]
    public async Task Empty_directory_argument_fails_before_execution_error_handling() {
        using var workspace = new TestWorkspace();

        var result = await MollycoddleTestHelper.ExecuteAsync(
            workspace.Repository, ["-dir=", $"-temppath={workspace.CacheDirectory}"]);

        // Windows reports an unhandled CLR exception using HRESULT 0xE0434352.
        result.ExitCode.ShouldBe(unchecked((int)0xE0434352), result.Diagnostics);
        result.StdErr.ShouldContain("System.ArgumentException", Case.Sensitive, result.Diagnostics);
        result.StdErr.ShouldContain("The path is empty.", Case.Sensitive, result.Diagnostics);
        result.StdOut.ShouldNotContain("No Violations, Mollycoddle Pass.", Case.Sensitive, result.Diagnostics);
        Directory.GetFileSystemEntries(workspace.CacheDirectory).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Warning_mode_reports_violations_but_returns_success(bool warningMode) {
        using var workspace = new TestWorkspace();
        workspace.WriteFile("forbidden.cs");
        string rules = workspace.WriteRules(new TestRule("IT_PROHIBITED", "MustNotExist", "**/*.cs"));

        var result = await MollycoddleTestHelper.ScanAsync(
            workspace, rules, warningMode ? ["-warnonly"] : []);

        result.AssertScan(warningMode ? 0 : 1, "IT_PROHIBITED");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Azure_output_preserves_issue_severity_and_completion(
        bool violation, bool warningMode) {
        using var workspace = new TestWorkspace();
        if (violation) {
            workspace.WriteFile("forbidden.cs");
        }
        string rules = workspace.WriteRules(new TestRule("IT_PROHIBITED", "MustNotExist", "**/*.cs"));
        string[] arguments = warningMode ? ["-formatter=azdo", "-warnonly"] : ["-formatter=azdo"];

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules, arguments);

        result.AssertScan(violation && !warningMode ? 1 : 0, violation ? ["IT_PROHIBITED"] : []);
        string completion = violation && !warningMode ? "Failed" : "Succeeded";
        result.StdOut.ShouldContain($"##vso[task.complete result={completion};]", Case.Sensitive, result.Diagnostics);
        if (violation) {
            result.StdOut.ShouldContain(
                $"##vso[task.logissue type={(warningMode ? "warning" : "error")}]", Case.Sensitive, result.Diagnostics);
        } else {
            result.StdOut.ShouldNotContain("##vso[task.logissue", Case.Sensitive, result.Diagnostics);
        }
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("-h")]
    [InlineData("/help")]
    public async Task Help_does_not_require_a_target_or_rules(string argument) {
        using var workspace = new TestWorkspace();

        var result = await MollycoddleTestHelper.ExecuteAsync(workspace.Repository, [
            argument, $"-temppath={workspace.CacheDirectory}"
        ]);

        result.ExitCode.ShouldBe(0, result.Diagnostics);
        result.StdErr.ShouldBeEmpty(result.Diagnostics);
        result.StdOut.ShouldContain("MollyCoddle, for when", Case.Sensitive, result.Diagnostics);
        Directory.GetFileSystemEntries(workspace.CacheDirectory).ShouldBeEmpty();
    }

    [Fact]
    public async Task Disabled_mode_does_not_validate_or_scan_the_target() {
        using var workspace = new TestWorkspace();

        var result = await MollycoddleTestHelper.ExecuteAsync(workspace.Repository, [
            $"-dir={Path.Combine(workspace.Root, "missing")}",
            $"-rulesfile={Path.Combine(workspace.RulesDirectory, "missing.molly")}",
            $"-temppath={workspace.CacheDirectory}", "-disabled"
        ]);

        result.ExitCode.ShouldBe(0, result.Diagnostics);
        result.StdOut.ShouldContain("MollyCoddle is disabled, returning success.", Case.Sensitive, result.Diagnostics);
        result.StdOut.ShouldNotContain("Violation:", Case.Sensitive, result.Diagnostics);
        result.StdErr.ShouldBeEmpty(result.Diagnostics);
        Directory.GetFileSystemEntries(workspace.CacheDirectory).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("directory", "Directory not found")]
    [InlineData("missing-rules", "Molly rules file was not found")]
    [InlineData("malformed-rules", "Invalid Json")]
    [InlineData("unknown-type", "ruleset file type is not known")]
    public async Task Execution_errors_are_explicit_and_return_minus_three(string kind, string message) {
        using var workspace = new TestWorkspace();
        string rules = workspace.WriteRules(new TestRule("IT_REQUIRED", "MustExist", "**/*.cs"));
        string target = workspace.Repository;
        if (kind == "directory") {
            target = Path.Combine(workspace.Root, "missing");
        } else if (kind == "missing-rules") {
            rules = Path.Combine(workspace.RulesDirectory, "missing.molly");
        } else if (kind == "malformed-rules") {
            rules = workspace.WriteRulesText("invalid.molly", "{ invalid json");
        } else if (kind == "unknown-type") {
            rules = workspace.WriteRulesText("invalid.txt", "not a rules file");
        }

        var result = await MollycoddleTestHelper.ExecuteAsync(workspace.Repository, [
            $"-dir={target}", $"-rulesfile={rules}", $"-temppath={workspace.CacheDirectory}"
        ]);

        result.ExitCode.ShouldBe(-3, result.Diagnostics);
        result.StdOut.ShouldContain(message, Case.Sensitive, result.Diagnostics);
        result.StdOut.ShouldContain("Error:", Case.Sensitive, result.Diagnostics);
        result.StdOut.ShouldNotContain("No Violations, Mollycoddle Pass.", Case.Sensitive, result.Diagnostics);
        result.StdErr.ShouldBeEmpty(result.Diagnostics);
    }
}
