using Xunit;

namespace mollycoddle.ITest;

public class DirectoryMatchingTests {

    // This test covers the existing behaviour.
    // Further investigation is required to determin whether this behaviour is expected/desired.
    [Theory]
    [InlineData(@".git\forbidden", 0)]
    [InlineData(@".vs\forbidden", 0)]
    [InlineData(@"src\.git\forbidden", 0)]
    [InlineData(@"src\.vs\forbidden", 0)]
    [InlineData(@"src\ordinary\forbidden", 1)]
    public async Task Directory_discovery_does_not_descend_into_git_or_vs(string path, int defects) {
        using var workspace = new TestWorkspace();
        workspace.CreateDirectory(path);
        string rules = workspace.WriteRules(new TestRule(
            "IT_DIRECTORY", "MustNotExist", "**/forbidden", "DirectoryValidationChecks"));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(defects, defects == 0 ? [] : ["IT_DIRECTORY"]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Required_directory_is_an_exact_rooted_path(bool exists) {
        using var workspace = new TestWorkspace();
        workspace.CreateDirectory(exists ? "src" : "src-other");
        string rules = workspace.WriteRules(new TestRule(
            "IT_DIRECTORY", "MustExist", @"%ROOT%\SRC", "DirectoryValidationChecks"));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(exists ? 0 : 1, exists ? [] : ["IT_DIRECTORY"]);
    }

    [Theory]
    [InlineData(@"src\forbidden", 1)]
    [InlineData(@"src\nested\forbidden", 1)]
    [InlineData(@"src\forbidden-near-miss", 0)]
    public async Task Directory_prohibition_matches_exact_segments(string path, int defects) {
        using var workspace = new TestWorkspace();
        workspace.CreateDirectory(path);
        string rules = workspace.WriteRules(new TestRule(
            "IT_DIRECTORY", "MustNotExist", "**/forbidden", "DirectoryValidationChecks"));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(defects, defects == 0 ? [] : ["IT_DIRECTORY"]);
    }

    [Theory]
    [InlineData(false, @"allowed\forbidden", 0)]
    [InlineData(false, @"allowed-other\forbidden", 1)]
    [InlineData(true, @"allowed\forbidden", 0)]
    [InlineData(true, @"allowed-other\forbidden", 1)]
    public async Task Directory_exceptions_and_bypasses_preserve_near_misses(
        bool fullBypass, string path, int defects) {
        using var workspace = new TestWorkspace();
        workspace.CreateDirectory(path);
        TestRule prohibition = new(
            "IT_DIRECTORY", fullBypass ? "MustNotExist" : "ProhibitedExcept",
            "**/forbidden", "DirectoryValidationChecks",
            fullBypass ? [] : ["**/allowed/forbidden"]);
        string rules = fullBypass
            ? workspace.WriteRules(prohibition,
                new("IT_BYPASS", "FullBypass", "**/allowed/**", "DirectoryValidationChecks"))
            : workspace.WriteRules(prohibition);

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(defects, defects == 0 ? [] : ["IT_DIRECTORY"]);
    }
}
