namespace mollycoddle.test {
    using System.IO;
    using Plisky.Diagnostics;
    using Plisky.Diagnostics.Listeners;
    using Plisky.Test;
    using Xunit;

    public class RulesFiles_IntegrationTests {
        private Bilge b = new Bilge();
        private UnitTestHelper u;

        public RulesFiles_IntegrationTests() {                   
            u = new UnitTestHelper();
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

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_PenBeMighty), "mollycoddle.testdata", "*.molly");

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
        public void RulesFile_ReadMe_ViolationIfNotExists() {

            string root = @"C:\MadeUpRoot";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("readmea.md", "bungle"); // violation

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_PenBeMighty), "mollycoddle.testdata", "*.molly");

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

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_OneLanguage), "mollycoddle.testdata", "*.molly");
            
           
            var sut = new MollyRuleFactory();            
            var rls = sut.LoadRulesFromFile(js);

            var m = new Molly(new MollyOptions());
            m.AddProjectStructure(mps);
            m.ImportRules(rls);

            var cr = m.ExecuteAllChecks();

            Assert.Equal(4,cr.DefectCount);
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

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_HotSauce), "mollycoddle.testdata", "*.molly");

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

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_HotSauce), "mollycoddle.testdata", "*.molly");

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
        public void RulesFile_GoodRoots_DetectsBadRoots() {

            string root = @"C:\MadeUpRoot";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");      // pass
            mps.WithRootedFolder("bannana"); // violation
            mps.WithRootedFolder("test");    // pass
            mps.WithRootedFolder("arfle");   // violation
            mps.WithRootedFolder("build");  // pass

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_GoodRoots), "mollycoddle.testdata", "*.molly");

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

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_GoodRoots), "mollycoddle.testdata", "rule.molly");

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
            mps.WithFile("c:\\mastermadeup\\master.editorconfig", "mock-editorconfig-contents");

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_EditorConfigMaster), "mollycoddle.testdata", "*.molly");


            var sut = new MollyRuleFactory();
            var rls = sut.LoadRulesFromFile(js);

            var m = new Molly(new MollyOptions() {
                MasterPath = @"c:\mastermadeup"
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
            mps.WithFile("c:\\mastermadeup\\master.gitignore", "mock-gitignore-contents");

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_GitIgnoreMaster), "mollycoddle.testdata", "*.molly");


            var sut = new MollyRuleFactory();
            var rls = sut.LoadRulesFromFile(js);

            var m = new Molly(new MollyOptions() {
                MasterPath = @"c:\mastermadeup"
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
            mps.WithFile("c:\\mastermadeup\\master.nuget.config", "mock-nugetconfig-contents");

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_NugetConfigMaster), "mollycoddle.testdata", "*.molly");


            var sut = new MollyRuleFactory();
            var rls = sut.LoadRulesFromFile(js);

            var m = new Molly(new MollyOptions() {
                MasterPath = @"c:\mastermadeup"
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
            mps.WithRootedFile("src\\bad.csproj", u.GetTestDataFromFile(TestResources.GetIdentifiers(TestResourcesReferences.CsProjBandPackage)));  // violation banned.package
            mps.WithRootedFile("src\\good.csproj", u.GetTestDataFromFile(TestResources.GetIdentifiers(TestResourcesReferences.CsProjSimpleNugetReferences)));   // pass

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_NoNaughtyNugets), "mollycoddle.testdata", "*.molly");

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
        public void PreciseLocationTests_DetectsNestedCSProj() {

            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\bob.sln");   // Forced Pass
            mps.WithRootedFile("src\\testproj\\testproj.csproj", "basil was here");    // pass
            mps.WithRootedFile("src_two\\testproj2\\testproj.csproj", "basil was here");   // pass
            mps.WithRootedFile("src\\testproj2\\nested\\testproj.csproj", "basil was here");  // Fail - nested projects

            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_thesolution), "mollycoddle.testdata", "*.molly");

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


            string js = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.MollyRule_thesolution), "mollycoddle.testdata", "*.molly");

            var sut = new MollyRuleFactory();
            var rls = sut.LoadRulesFromFile(js);

            var m = new Molly(new MollyOptions());
            m.AddProjectStructure(mps);
            m.ImportRules(rls);

            var cr = m.ExecuteAllChecks();

            Assert.Equal(1, cr.DefectCount);
        }

    }
}