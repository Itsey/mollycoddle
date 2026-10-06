using Shouldly;
using Xunit;

namespace mollycoddle.ITest;

public class NugetMatchingTests {
    private const string PROJECT_WITH_BANNED_PACKAGE = """
        <Project Sdk="Microsoft.NET.Sdk">
          <ItemGroup>
            <PackageReference Include="banned.package" Version="1.2.3" />
          </ItemGroup>
        </Project>
        """;

    [Theory]
    [InlineData(@"src\project.csproj", 1)]
    [InlineData(@"src\nested\project.csproj", 1)]
    [InlineData(@"other\project.csproj", 0)]
    [InlineData(@"src\project.txt", 0)]
    public async Task Package_checks_select_only_matching_project_paths(string path, int defects) {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(path, PROJECT_WITH_BANNED_PACKAGE);
        string rules = workspace.WriteRules(new TestRule(
            "IT_PACKAGE", "ProhibitedPackagesList", @"%ROOT%\SRC\**\*.CSPROJ",
            "NugetValidationChecks", ["banned.package"]));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(defects, defects == 0 ? [] : ["IT_PACKAGE"]);
        if (defects > 0) {
            result.StdOut.ShouldContain("banned.package", Case.Sensitive, result.Diagnostics);
        }
    }

    [Fact]
    public async Task Allowed_package_in_selected_project_passes() {
        using var workspace = new TestWorkspace();
        workspace.WriteFile(@"src\project.csproj",
            PROJECT_WITH_BANNED_PACKAGE.Replace("banned.package", "allowed.package"));
        string rules = workspace.WriteRules(new TestRule(
            "IT_PACKAGE", "ProhibitedPackagesList", "**/*.csproj",
            "NugetValidationChecks", ["banned.package"]));

        var result = await MollycoddleTestHelper.ScanAsync(workspace, rules);

        result.AssertScan(0);
    }
}
