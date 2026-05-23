namespace mollycoddle.test;

using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

public class FileStructureCheckTests {
    protected Bilge b = new();
    private const string DUMMYRULE = "dummy";

    public FileStructureCheckTests() {
    }

    [Fact]
    [Integration]
    public void Compare_target_with_master_file_works() {
        b.Info.Flow();

        string root = @"c:\MadeUpPath";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\.gitignore", "gitignorefilecontents");
        mps.WithFile("%MASTEROOT%\\master.gitignore", "gitignorefilecontents");

        var sut = new MockFileStructureChecker(mps);
        sut.AssignCompareWithCommonAction("**/.gitignore", "%MASTEROOT%\\master.gitignore", DUMMYRULE);

        var cr = sut.Check();

        Assert.Equal(0, cr.DefectCount);
    }

    [Fact]
    [Integration]
    public void File_must_exist_rule_defect_if_no_file() {
        b.Info.Flow();

        string root = @"c:\MadeUpPath";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\mytestfile.bob", "basil was here");
        var sut = new MockFileStructureChecker(mps);
        sut.AssignMustExistAction(DUMMYRULE, "**/*.cs");

        var cr = sut.Check();

        Assert.Equal(1, cr.DefectCount);
    }

    [Fact]
    [Integration]
    public void File_must_exist_rule_no_defect_if_file() {
        b.Info.Flow();

        string root = @"c:\MadeUpPath";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\mytestfile.cs", "basil was here");
        var sut = new MockFileStructureChecker(mps);
        sut.AssignMustExistAction(DUMMYRULE, "**/*.cs");

        var cr = sut.Check();

        Assert.Equal(0, cr.DefectCount);
    }

    [Fact]
    [Integration]
    public void File_must_not_contain_rule_defect_if_value_found() {
        b.Info.Flow();

        string root = @"c:\MadeUpPath";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\mytestfile.cs", "basil was here");
        var sut = new MockFileStructureChecker(mps);

        sut.AssignFileMustNotContainAction(DUMMYRULE, " **/*.cs", "basil");

        var cr = sut.Check();
        Assert.Equal(1, cr.DefectCount);
    }

    [Fact]
    [Integration]
    public void File_must_not_exist_rule_no_defect_if_exception_specified() {
        b.Info.Flow();

        string root = @"c:\MadeUpPath";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\mytestfile.bob", "basil was here");
        var sut = new MockFileStructureChecker(mps);

        sut.AssignMustNotExistAction(DUMMYRULE, new MatchWithSecondaryMatches("**/*.bob") {
            SecondaryList = new string[] { "**\\mytestfile.bob" }
        });
        var cr = sut.Check();

        Assert.Equal(0, cr.DefectCount);
    }

    [Fact]
    [Integration]
    public void File_must_not_exist_rule_defect_if_file_exists() {
        b.Info.Flow();

        string root = @"c:\MadeUpPath";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\mytestfile.bob", "basil was here");
        var sut = new MockFileStructureChecker(mps);

        sut.AssignMustNotExistAction(DUMMYRULE, new MatchWithSecondaryMatches("**/*.bob"));

        var cr = sut.Check();

        Assert.Equal(1, cr.DefectCount);
    }

    [Fact]
    [Integration]
    public void File_must_not_exist_rule_no_defect_if_file_does_not_exist() {
        b.Info.Flow();

        string root = @"c:\MadeUpPath";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\mytestfile.bob", "basil was here");
        var sut = new MockFileStructureChecker(mps);
        sut.AssignMustNotExistAction(DUMMYRULE, new MatchWithSecondaryMatches("**/*.cs"));

        var cr = sut.Check();

        Assert.Equal(0, cr.DefectCount);
    }

    [Fact]
    [Build(BuildType.CI)]
    public void File_system_master_bypass_no_defect() {
        string root = @"C:\MadeUpFolder";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        _ = mps.WithRootedFolder("pickle");
        mps.WithRootedFile("src\\testproj\\xx\\testproj.csproj", "basil was here");   // fail
        mps.WithRootedFile("src_two\\testproj2\\testproj.csproj", "basil was here");  // Pass
        mps.WithRootedFile("src\\testproj2\\nested\\testproj.csproj", "basil was here");  // Fail - nested projects

        var sut = new MockFileStructureChecker(mps);
        sut.AddFullBypass("**");
        string[] secondary = new string[] { "**/src*/*/*.csproj" };
        sut.AssignIfItExistsItMustBeHereAction(DUMMYRULE, new MatchWithSecondaryMatches("**/*.csproj") { SecondaryList = secondary });
        var cr = sut.Check();

        Assert.Equal(0, cr.DefectCount);
    }

    [Fact]
    [Fresh]
    public void If_exists_must_be_here_action_adds_violation_on_fail() {
        string root = @"C:\MadeUpFolder";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        _ = mps.WithRootedFolder("pickle");
        mps.WithRootedFile("src\\testproj\\testproj.csproj", "basil was here");   // Pass
        mps.WithRootedFile("src_two\\testproj2\\testproj.csproj", "basil was here");  // Pass
        mps.WithRootedFile("pickle\\testproj2\\testproj.csproj", "basil was here");  // Fail - not under src

        var sut = new MockFileStructureChecker(mps);
        string[] secondary = new string[] { "**/src*/*/*.csproj" };
        sut.AssignIfItExistsItMustBeHereAction("dummyrule", new MatchWithSecondaryMatches("**/*.csproj") { SecondaryList = secondary });
        var cr = sut.Check();

        Assert.Equal(1, cr.DefectCount);
    }

    [Fact]
    [Integration]
    public void Must_exist_in_specific_location_fails_if_not_match_secondary() {
        string root = @"C:\MadeUpFolder";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        _ = mps.WithRootedFolder("pickle");
        mps.WithRootedFile("src\\testproj\\testproj.csproj", "basil was here");   // Pass
        mps.WithRootedFile("src_two\\testproj2\\testproj.csproj", "basil was here");  // Pass
        mps.WithRootedFile("src\\testproj2\\nested\\testproj.csproj", "basil was here");  // Fail - nested projects

        var sut = new MockFileStructureChecker(mps);
        string[] secondary = new string[] { "**/src*/*/*.csproj" };
        sut.AssignIfItExistsItMustBeHereAction(DUMMYRULE, new MatchWithSecondaryMatches("**/*.csproj") { SecondaryList = secondary });
        var cr = sut.Check();

        Assert.Equal(1, cr.DefectCount);
    }

    [Fact]
    [Integration]
    public void Must_exist_in_specific_location_no_defect_if_valid() {
        string root = @"C:\MadeUpFolder";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\testproj\\testproj.csproj", "basil was here");
        mps.WithRootedFile("src_two\\testproj2\\testproj.csproj", "basil was here");

        var sut = new MockFileStructureChecker(mps);
        string[] secondary = new string[] { "**/src*/*/*.csproj" };
        sut.AssignIfItExistsItMustBeHereAction(DUMMYRULE, new MatchWithSecondaryMatches("**/*.csproj") { SecondaryList = secondary });
        var cr = sut.Check();

        Assert.Equal(0, cr.DefectCount);
    }

    [Fact]
    [Fresh]
    public void File_must_not_contain_defect_if_value_present() {
        b.Info.Flow();

        string root = @"C:\MadeUpFolder";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\mytestfile.cs", "basil was here");
        var sut = new MockFileStructureChecker(mps);
        sut.AssignFileMustNotContainAction("dummyrule", "**/*.cs", "basil");

        var cr = sut.Check();

        Assert.Equal(1, cr.DefectCount);
    }
}