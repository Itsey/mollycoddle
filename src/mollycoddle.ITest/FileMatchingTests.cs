using Shouldly;
using Xunit;

namespace mollycoddle.ITest;

public class FileMatchingTests {

    // This test covers the existing behaviour.
    // Further investigation is required to determin whether this behaviour is expected/desired.
    [Theory]
    [InlineData("**/*.cs", "file.cs", true)]
    [InlineData("**/*.cs", @"src\nested\file.cs", true)]
    [InlineData("**/*.cs", @"src\file.txt", false)]
    [InlineData("%ROOT%/src/*.cs", @"src\file.cs", true)]
    [InlineData("%ROOT%/src/*.cs", @"src\nested\file.cs", false)]
    [InlineData(@"%ROOT%\src\**\*.cs", @"src\file.cs", true)]
    [InlineData(@"%ROOT%\src\**\*.cs", @"src\nested\file.cs", true)]
    [InlineData(@"%ROOT%\src\**\*.cs", @"other\file.cs", false)]
    [InlineData("%ROOT%/SRC/**/*.CS", @"src\nested\File.Cs", true)]
    [InlineData("**/file?.cs", @"src\file1.cs", true)]
    [InlineData("**/file?.cs", @"src\file10.cs", false)]
    [InlineData("**/*.{cs,csproj}", @"src\project.csproj", true)]
    [InlineData("**/*.{cs,csproj}", @"src\file.txt", false)]
    [InlineData("**/file[0-9].cs", @"src\file9.cs", true)]
    [InlineData("**/file[0-9].cs", @"src\filea.cs", false)]
    [InlineData("src/**/*.cs", @"src\file.cs", false)]
    [InlineData("*.cs", "file.cs", false)]
    public async Task Required_and_prohibited_rules_select_expected_paths(
        string pattern, string path, bool matches) {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(path);
        string rules = workspace.WriteRules(
            new("IT_REQUIRED", "MustExist", pattern),
            new("IT_PROHIBITED", "MustNotExist", pattern));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(1, matches ? "IT_PROHIBITED" : "IT_REQUIRED");
    }

    [Fact]
    public async Task Empty_repository_fails_required_but_not_prohibited_rule() {
        using var workspace = new TestWorkspace();
        string rules = workspace.WriteRules(
            new("IT_REQUIRED", "MustExist", "**/*.cs"),
            new("IT_PROHIBITED", "MustNotExist", "**/*.cs"));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(1, "IT_REQUIRED");
    }

    [Theory]
    [InlineData(@"bin\file.cs", 0)]
    [InlineData(@"src\bin\Debug\file.cs", 0)]
    [InlineData(@"src\binary\file.cs", 1)]
    [InlineData(@"src\file.cs", 1)]
    public async Task Full_bypass_excludes_only_matching_paths(string path, int defects) {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(path);
        string rules = workspace.WriteRules(
            new("IT_BYPASS", "FullBypass", "**/bin/**"),
            new("IT_PROHIBITED", "MustNotExist", "**/*.cs"));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(defects, defects == 0 ? [] : ["IT_PROHIBITED"]);
    }

    [Theory]
    [InlineData(@"src\bin\file.cs", 0)]
    [InlineData(@"src\binary\file.cs", 1)]
    [InlineData(@"src\file.cs", 1)]
    public async Task Prohibition_exception_excludes_only_matching_paths(string path, int defects) {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(path);
        string rules = workspace.WriteRules(new TestRule(
            "IT_PROHIBITED", "MustNotExist", "**/*.cs", AdditionalData: ["**/bin/**"]));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(defects, defects == 0 ? [] : ["IT_PROHIBITED"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Bypassed_files_do_not_hide_a_nonbypassed_violation(bool useGitignore) {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(@"bin\ignored.cs");
        string actual = workspace.WriteFile(@"src\violation.cs");
        workspace.WriteFile(".gitignore", "bin/\n");
        string rules = workspace.WriteRules(new TestRule(
            "IT_PROHIBITED", "MustNotExist", "**/*.cs",
            AdditionalData: [useGitignore ? @"%ROOT%\.gitignore" : "**/bin/**"]));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(1, "IT_PROHIBITED");
        result.StdOut.ShouldContain(actual.ToLowerInvariant(), Case.Sensitive, result.Diagnostics);
        result.StdOut.ShouldNotContain("ignored.cs", Case.Sensitive, result.Diagnostics);
    }

    [Fact]
    public async Task Gitignore_exception_allows_ignored_only_files() {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(@"bin\ignored.cs");
        workspace.WriteFile(".gitignore", "bin/\n");
        string rules = workspace.WriteRules(new TestRule(
            "IT_PROHIBITED", "MustNotExist", "**/*.cs", AdditionalData: [@"%ROOT%\.gitignore"]));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(0);
    }

    [Theory]
    [InlineData(@"src\project\project.csproj", 0)]
    [InlineData(@"src\project.csproj", 1)]
    [InlineData(@"src\project\nested\project.csproj", 1)]
    [InlineData(@"src\file.txt", 0)]
    public async Task Placement_rule_checks_only_selected_files(string path, int defects) {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(path);
        string rules = workspace.WriteRules(new TestRule(
            "IT_PLACEMENT", "IfExistMustBeHere", "**/*.csproj",
            AdditionalData: ["**/src/*/*.csproj"]));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(defects, defects == 0 ? [] : ["IT_PLACEMENT"]);
    }

    [Fact]
    public async Task Several_matching_files_produce_one_defect_per_failed_action() {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(@"src\first.cs");
        workspace.WriteFile(@"src\second.cs");
        workspace.WriteFile(@"src\third.txt");
        string rules = workspace.WriteRules(
            new("IT_CS", "MustNotExist", "**/*.cs"),
            new("IT_TEXT", "MustNotExist", "**/*.txt"));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(2, "IT_CS", "IT_TEXT");
    }
}
