namespace mollycoddle.test;

using Minimatch;
using Shouldly;
using Xunit;

public class MinimatchTests {
    [Theory]
    [InlineData("**/src/*.cs", @"C:\repo\src\file.cs", true)]
    [InlineData("**/src/*.cs", @"C:\repo\src\nested\file.cs", false)]
    [InlineData("**/src/file?.cs", @"C:\repo\src\file1.cs", true)]
    [InlineData("**/src/file?.cs", @"C:\repo\src\file10.cs", false)]
    [InlineData("**/src/**/*.cs", @"C:\repo\src\file.cs", true)]
    [InlineData("**/src/**/*.cs", @"C:\repo\src\nested\file.cs", true)]
    [InlineData("**/src/**/*.cs", @"C:\repo\src\nested\file.txt", false)]
    [InlineData("**/*.sln", @"C:\repo\src\mollycoddle.sln", true)]
    [InlineData("**\\src\\*.sln", @"C:\repo\src\mollycoddle.sln", true)]
    [InlineData("**/*.CS", @"C:\repo\src\mollycoddle.cs", true)]
    [InlineData("C:\\repo\\src\\file.cs", @"C:\repo\src\fileXcs", false)]
    [InlineData("*.cs", "file.cs", true)]
    [InlineData("*.cs", "file.txt", false)]
    [InlineData("*.cs", "src/file.cs", false)]
    [InlineData("**/*.cs", "file.cs", true)]
    [InlineData("**/*.cs", "src/nested/file.cs", true)]
    [InlineData("**/*.cs", "src/nested/file.txt", false)]
    [InlineData(@"src\**\*.cs", @"src\file.cs", true)]
    [InlineData(@"src\**\*.cs", @"src\nested\file.cs", true)]
    [InlineData(@"src\**\*.cs", @"other\nested\file.cs", false)]
    [InlineData("src/**/*.cs", "src/file.cs", true)]
    [InlineData("src/**/*.cs", "src/nested/file.cs", true)]
    [InlineData("src/**/*.cs", "other/nested/file.cs", false)]
    [InlineData("src/**/test", "src/test", true)]
    [InlineData("src/**/test", "src/nested/deeper/test", true)]
    [InlineData("src/**/test", "src/nested/testing", false)]
    [InlineData("src/**/test", "src/nested/test/file.cs", false)]
    [InlineData("**/bin/**", "bin/file.dll", true)]
    [InlineData("**/bin/**", "src/bin/Debug/file.dll", true)]
    [InlineData("**/bin/**", "src/binary/file.dll", false)]
    [InlineData("**/*.{cs,csproj}", "file.cs", true)]
    [InlineData("**/*.{cs,csproj}", "src/nested/project.csproj", true)]
    [InlineData("**/*.{cs,csproj}", "src/file.txt", false)]
    [InlineData("foo?.cs", "foo1.cs", true)]
    [InlineData("foo?.cs", "foo.cs", false)]
    [InlineData("foo?.cs", "foo12.cs", false)]
    [InlineData("foo[0-9].cs", "foo0.cs", true)]
    [InlineData("foo[0-9].cs", "foo9.cs", true)]
    [InlineData("foo[0-9].cs", "fooa.cs", false)]
    [InlineData("foo[0-9].cs", "foo10.cs", false)]
    [InlineData("**/src/[ab]ile.cs", @"C:\repo\src\aile.cs", true)]
    [InlineData("**/src/[ab]ile.cs", @"C:\repo\src\bile.cs", true)]
    [InlineData("**/src/[ab]ile.cs", @"C:\repo\src\cile.cs", false)]
    [InlineData("**/src/[ab]ile.cs", @"C:\repo\src\afile.cs", false)]
    [InlineData(@"C:\repo\src\**\*.cs", @"C:\repo\src\file.cs", true)]
    [InlineData(@"C:\repo\src\**\*.cs", @"C:\repo\src\nested\file.cs", true)]
    [InlineData(@"C:\repo\src\**\*.cs", @"D:\repo\src\nested\file.cs", false)]
    [InlineData(@"C:\repo\src\**\*.cs", @"C:\other\src\nested\file.cs", false)]
    [InlineData(@"C:\repo\src\**\*.cs", @"C:\repo\src\nested\file.txt", false)]
    [InlineData("src/**/*.cs", @"C:\repo\src\file.cs", false)]
    [InlineData("", "", true)]
    [InlineData("", "file.cs", false)]
    [InlineData("*.cs", "", false)]
    public void Pattern_match_obeys_expected_glob_semantics(string pattern, string path, bool expected) {
        var options = new Options {
            AllowWindowsPaths = true,
            IgnoreCase = true
        };
        var matcher = new Minimatcher(pattern, options);
        var filter = Minimatcher.CreateFilter(pattern, options);

        matcher.IsMatch(path).ShouldBe(expected);
        filter(path).ShouldBe(expected);
    }

    [Theory]
    [InlineData("README.md", "readme.md", true, true)]
    [InlineData("README.md", "readme.md", false, false)]
    [InlineData("README.md", "README.md", false, true)]
    [InlineData("SRC/**/*.cs", "src/nested/file.cs", true, true)]
    [InlineData("SRC/**/*.cs", "src/nested/file.cs", false, false)]
    public void Case_sensitivity_obeys_configured_option(string pattern, string path, bool ignoreCase, bool expected) {
        var options = new Options {
            AllowWindowsPaths = true,
            IgnoreCase = ignoreCase
        };

        new Minimatcher(pattern, options).IsMatch(path).ShouldBe(expected);
        Minimatcher.CreateFilter(pattern, options)(path).ShouldBe(expected);
    }

    [Theory]
    [InlineData(@"src\file.cs", 0)]
    [InlineData(@"src\nested\file.cs", 0)]
    [InlineData(@"other\file.cs", 1)]
    [InlineData(@"src\file.txt", 1)]
    public void Must_exist_expands_repository_root_in_recursive_pattern(string file, int expectedDefects) {
        var structure = MockProjectStructure.Get().WithRoot(@"C:\repo");
        structure.WithRootedFile(file);
        var checker = new MockFileStructureChecker(structure);
        checker.AssignMustExistAction("rooted-files", @"%ROOT%\src\**\*.cs");

        checker.Check().DefectCount.ShouldBe(expectedDefects);
    }

    [Theory]
    [InlineData(@"bin\file.cs", 0)]
    [InlineData(@"src\bin\Debug\file.cs", 0)]
    [InlineData(@"src\file.cs", 1)]
    [InlineData(@"src\binary\file.cs", 1)]
    public void Selective_bypass_excludes_only_matching_files(string file, int expectedDefects) {
        var structure = MockProjectStructure.Get().WithRoot(@"C:\repo");
        structure.WithRootedFile(file);
        var checker = new MockFileStructureChecker(structure);
        checker.AddFullBypass("**/bin/**");
        checker.AssignMustNotExistAction("prohibited-files", new MatchWithSecondaryMatches("**/*.cs"));

        checker.Check().DefectCount.ShouldBe(expectedDefects);
    }

    [Theory]
    [InlineData(@"src\bin\Debug\file.cs", 0)]
    [InlineData(@"src\file.cs", 1)]
    public void Must_not_exist_exception_excludes_only_matching_files(string file, int expectedDefects) {
        var structure = MockProjectStructure.Get().WithRoot(@"C:\repo");
        structure.WithRootedFile(file);
        var checker = new MockFileStructureChecker(structure);
        checker.AssignMustNotExistAction("prohibited-files", new MatchWithSecondaryMatches("**/*.cs") {
            SecondaryList = ["**/bin/**"]
        });

        checker.Check().DefectCount.ShouldBe(expectedDefects);
    }
}
