namespace mollycoddle.test;

using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

public class RulesFiles_IntegrationTests {
    private Bilge b = new Bilge();
    private TestUtilities tu = new TestUtilities();

    public RulesFiles_IntegrationTests() {
    }

    ~RulesFiles_IntegrationTests() {
        tu.CloseAllFiles();
    }

    [Integration]
    [Build(BuildType.Release)]
    [Integration]
    [Fact]
    public void PreciseLocationTests_DetectsNestedCSProj() {
        string root = @"C:\MadeUpFolder";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\bob.sln");   // Forced Pass
        mps.WithRootedFile("src\\testproj\\testproj.csproj", "basil was here");    // pass
        mps.WithRootedFile("src_two\\testproj2\\testproj.csproj", "basil was here");   // pass
        mps.WithRootedFile("src\\testproj2\\nested\\testproj.csproj", "basil was here");  // Fail - nested projects

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_TheSolution);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions());
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(1, cr.DefectCount);
    }

    [Integration]
    [Build(BuildType.Release)]
    [Integration]
    [Fact]
    public void PreciseLocationTests_RequiresSolutionFile() {
        string root = @"C:\MadeUpFolder";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");   // FAIL no solution file

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_TheSolution);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions());
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(1, cr.DefectCount);
    }

    [Build(BuildType.Release)]
    [Integration]
    [Fact]
    public void RulesFile_GoodRoots_DetectsBadRoots() {
        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");      // pass
        mps.WithRootedFolder("bannana"); // violation
        mps.WithRootedFolder("test");    // pass
        mps.WithRootedFolder("arfle");   // violation
        mps.WithRootedFolder("build");  // pass

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_GoodRoots);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions());
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(2, cr.DefectCount);
    }

    [Build(BuildType.Release)]
    [Integration]
    [Fact]
    public void RulesFile_GoodRoots_PassesWithGoodRoots() {
        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");    // pass
        mps.WithRootedFolder("test");   // pass
        mps.WithRootedFolder("build");  // pass

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_GoodRoots);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions());
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(0, cr.DefectCount);
    }

    [Build(BuildType.Release)]
    [Integration]
    [Fact]
    public void RulesFile_HotSauce_DetectsBadPath() {
        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");           // pass
        mps.WithRootedFolder("src\\src");      // fail
        mps.WithRootedFolder("test");          // pass

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_HotSauce);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions());
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(1, cr.DefectCount);
    }

    [Build(BuildType.Release)]
    [Integration]
    [Fact]
    public void RulesFile_HotSauce_PassesCorrectPath() {
        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");      // pass

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_HotSauce);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions());
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(0, cr.DefectCount);
    }

    [Build(BuildType.Release)]
    [Integration]
    [Theory]
    [InlineData("mock-editorconfig-contents", 0)]
    [InlineData("invalid-contents", 1)]
    public void RulesFile_MasterEditorConfig_Works(string ecfilecontents, int defectCount) {
        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");

        mps.WithRootedFile("src\\.editorconfig", ecfilecontents);
        mps.WithFile("c:\\primarymadeup\\common.editorconfig", "mock-editorconfig-contents");

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_EditorConfigSample);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions() {
            PrimaryFilePath = @"c:\primarymadeup"
        });
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(defectCount, cr.DefectCount);
    }

    [Build(BuildType.Release)]
    [Integration]
    [Theory]
    [InlineData("mock-gitignore-contents", 0)]
    [InlineData("invalid-contents", 1)]
    public void RulesFile_MasterGitIgnore_Works(string ecfilecontents, int defectCount) {
        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");

        mps.WithRootedFile(".gitignore", ecfilecontents);
        mps.WithFile("c:\\mastermadeup\\common.gitignore", "mock-gitignore-contents");

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_GitIgnoreMaster);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions() {
            PrimaryFilePath = @"c:\mastermadeup"
        });
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(defectCount, cr.DefectCount);
    }

    [Build(BuildType.Release)]
    [Integration]
    [Theory]
    [InlineData("mock-nugetconfig-contents", 0)]
    [InlineData("invalid-contents", 1)]
    public void RulesFile_MasterNugetConfig_Works(string ecfilecontents, int defectCount) {
        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");

        mps.WithRootedFile("src\\nuget.config", ecfilecontents);
        mps.WithFile("c:\\mastermadeup\\common.nuget.config", "mock-nugetconfig-contents");

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_NugetConfigMaster);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions() {
            PrimaryFilePath = @"c:\mastermadeup"
        });
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(defectCount, cr.DefectCount);
    }

    [Integration]
    [Build(BuildType.Release)]
    [Integration]
    [Fact]
    public void RulesFile_NoNaughtyNugets_Works() {
        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");

        mps.WithRootedFile("src\\bad.csproj", tu.GetTestDataFileContent(TestResourcesReferences.CsProjBandPackage));  // violation banned.package
        mps.WithRootedFile("src\\good.csproj", tu.GetTestDataFileContent(TestResourcesReferences.CsProjSimpleNugetReferences));   // pass

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_NoNaughtyNugets);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions());
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(1, cr.DefectCount);
    }

    [Build(BuildType.Release)]
    [Integration]
    [Fact]
    public void RulesFile_OneLanguageToRuleThemAll_Works() {
        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");
        mps.WithRootedFolder("src\\project.test");
        mps.WithRootedFile("src\\file.vb", "dummyfilecontents");  // violation
        mps.WithRootedFile("src\\file.py", "dummyfilecontents");   // violation
        mps.WithRootedFile("src\\file.java", "dummyfilecontents"); // violation
        mps.WithRootedFile("src\\file.vbproj", "dummyfilecontents"); // violation
        mps.WithRootedFile("src\\file.cs", "dummyfilecontents");   // Pass
        mps.WithRootedFile("src\\file.txt", "dummyfilecontents");   // Pass
        mps.WithRootedFile("src\\file.csproj", "dummyfilecontents");   // Pass

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_OneLanguage);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions());
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(4, cr.DefectCount);
    }

    [Build(BuildType.Release)]
    [Integration]
    [Fact]
    public void RulesFile_ReadMe_ViolationIfNotExists() {
        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");
        mps.WithRootedFile("readmea.md", "bungle"); // violation

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_PenBeMighty);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions());
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(1, cr.DefectCount);
    }

    [Build(BuildType.Release)]
    [Integration]
    [Fact]
    public void RulesFile_ReadMe_WorksIfExists() {
        // readme.md must exist

        string root = @"C:\MadeUpRoot";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");
        mps.WithRootedFile("readme.md", "bungle"); // Pass

        string js = tu.GetTestDataFile(TestResourcesReferences.MollyRule_PenBeMighty);

        var sut = new MollyRuleFactory();
        var rls = sut.LoadRulesFromFile(js);

        var m = new Molly(new MollyOptions());
        m.AddProjectStructure(mps);
        m.ImportRules(rls);

        var cr = m.ExecuteAllChecks();

        Assert.Equal(0, cr.DefectCount);
    }
}