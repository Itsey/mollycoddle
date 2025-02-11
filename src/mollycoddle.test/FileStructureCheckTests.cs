﻿namespace mollycoddle.test;

using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

public class FileStructureCheckTests {
    protected Bilge b = new();
    private const string DUMMYRULE = "dummy";

    public FileStructureCheckTests() {
    }

    [Fact(DisplayName = nameof(CompareWithMasterFile_RuleWorks))]
    [Integration]
    public void CompareWithMasterFile_RuleWorks() {
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
    public void FileMustExist_FailsIfFileDoesNotExist() {
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
    public void FileMustExist_PassesIfFileExists() {
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

    [Fact(DisplayName = nameof(CompareWithMasterFile_RuleWorks))]
    [Integration]
    public void FileMustNotContain_RuleWorks() {
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
    public void FileMustNotExist_AllowsForExceptions() {
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
    public void FileMustNotExist_FailsIfFileExists() {
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
    public void FileMustNotExist_PassesIfFileDoesNotExist() {
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
    public void FileSystem_MasterBypass_Works() {
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
    public void IfExistsMustBeHere_Action_AddsViolationOnFail() {
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
    public void MustExistInSpecificLocation_FailsIfNotMatchSecondary() {
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
    public void MustExistInSpecificLocation_PassesIfValid() {
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
    public void OneLanguage_FileStructureTest_Passes() {
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