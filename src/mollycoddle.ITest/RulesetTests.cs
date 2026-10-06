using Shouldly;
using Xunit;

namespace mollycoddle.ITest;

public class RulesetTests {
    [Fact]
    public async Task Ruleset_resolves_relative_rules_and_applies_cross_file_bypasses() {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(@"bin\ignored.cs");
        string violation = workspace.WriteFile(@"src\violation.cs");
        workspace.WriteRulesFile(@"nested\prohibited.molly",
            new TestRule("IT_PROHIBITED", "MustNotExist", "**/*.cs"));
        workspace.WriteRulesFile("bypass.molly", new TestRule("IT_BYPASS", "FullBypass", "**/bin/**"));
        string rules = workspace.WriteRulesText("combined.mollyset",
            "# local characterization rules\nnested\\prohibited.molly\nbypass.molly\n");

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(1, "IT_PROHIBITED");
        result.StdOut.ShouldContain(violation.ToLowerInvariant(), Case.Sensitive, result.Diagnostics);
        result.StdOut.ShouldNotContain("ignored.cs", Case.Sensitive, result.Diagnostics);
    }

    [Theory]
    [InlineData(".editorconfig")]
    [InlineData(@"src\.gitignore")]
    [InlineData(@".git\fixture.cs")]
    [InlineData(@".vs\fixture.cs")]
    public async Task Filesystem_discovery_includes_dotfiles_and_tool_metadata_files(string path) {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(path);
        string rules = workspace.WriteRules(new TestRule("IT_REQUIRED", "MustExist", @"%ROOT%\" + path));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(0);
    }
}
