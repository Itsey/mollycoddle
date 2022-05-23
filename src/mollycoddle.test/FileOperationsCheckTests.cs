using Plisky.Diagnostics;
using Xunit;

namespace mollycoddle.test {

    public class FileOperationsCheckTests {
        protected Bilge b = new Bilge();
        private const string DUMMYRULE = "dummy";

        [Fact]
        public void MustExistInSpecificLocation_PassesIfValid() {
            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\testproj\\testproj.csproj", "basil was here");
            mps.WithRootedFile("src_two\\testproj2\\testproj.csproj", "basil was here");

            var sut = new MockFileStructureChecker(mps);
            string[] secondary = new string[] { "**/src*/*/*.csproj" };
            sut.AssignIfItExistsItMustBeHereAction(DUMMYRULE, new MatchWithSecondaryMatches("**/*.csproj") { SecondaryList = secondary });
            var cr = sut.Check();

            Assert.Equal(0, cr.DefectCount);

        }

        [Fact]
        public void MustExistInSpecificLocation_FailsIfNotMatchSecondary() {
            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFolder("pickle");
            mps.WithRootedFile("src\\testproj\\testproj.csproj", "basil was here");   // Pass
            mps.WithRootedFile("src_two\\testproj2\\testproj.csproj", "basil was here");  // Pass
            mps.WithRootedFile("src\\testproj2\\nested\\testproj.csproj", "basil was here");  // Fail - nested projects

            var sut = new MockFileStructureChecker(mps);
            string[] secondary = new string[] { "**/src*/*/*.csproj" };
            sut.AssignIfItExistsItMustBeHereAction(DUMMYRULE, new MatchWithSecondaryMatches("**/*.csproj") { SecondaryList = secondary });
            var cr = sut.Check();

            Assert.Equal(1, cr.DefectCount);

        }

   

        [Fact(DisplayName = nameof(CompareWithMasterFile_RuleWorks))]
        public void CompareWithMasterFile_RuleWorks() {
            b.Info.Flow();

            string root = @"c:\MadeUpPath";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\.gitignore", "gitignorefilecontents");
            mps.WithFile("%MASTEROOT%\\master.gitignore", "gitignorefilecontents");

            var sut = new MockFileStructureChecker(mps);
            sut.AssignCompareWithMasterAction("**/.gitignore", "%MASTEROOT%\\master.gitignore", "DummyRule");

            var cr = sut.Check();

            Assert.Equal(0, cr.DefectCount);
        }

        [Fact]
        public void FileMustExist_FailsIfFileDoesNotExist() {
            b.Info.Flow();

            string root = @"c:\MadeUpPath";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\mytestfile.bob", "basil was here");
            var sut = new MockFileStructureChecker(mps);
            sut.AssignMustExistAction(DUMMYRULE, "**/*.cs");

            var cr = sut.Check();

            Assert.Equal(1, cr.DefectCount);
        }

        [Fact]
        public void FileMustNotExist_PassesIfFileDoesNotExist() {
            b.Info.Flow();

            string root = @"c:\MadeUpPath";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\mytestfile.bob", "basil was here");
            var sut = new MockFileStructureChecker(mps);
            sut.AssignMustNotExistAction(DUMMYRULE, new MatchWithSecondaryMatches("**/*.cs"));

            var cr = sut.Check();

            Assert.Equal(0, cr.DefectCount);
        }

        [Fact]
        public void FileMustNotExist_FailsIfFileExists() {
            b.Info.Flow();

            string root = @"c:\MadeUpPath";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\mytestfile.bob", "basil was here");
            var sut = new MockFileStructureChecker(mps);
            
            sut.AssignMustNotExistAction(DUMMYRULE, new MatchWithSecondaryMatches("**/*.bob"));

            var cr = sut.Check();

            Assert.Equal(1, cr.DefectCount);
        }



        [Fact]
        public void FileMustExist_PassesIfFileExists() {
            b.Info.Flow();

            string root = @"c:\MadeUpPath";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\mytestfile.cs", "basil was here");
            var sut = new MockFileStructureChecker(mps);
            sut.AssignMustExistAction(DUMMYRULE, "**/*.cs");

            var cr = sut.Check();

            Assert.Equal(0, cr.DefectCount);
        }

        [Fact(DisplayName = nameof(CompareWithMasterFile_RuleWorks))]
        public void FileMustNotContain_RuleWorks() {
            b.Info.Flow();

            string root = @"c:\MadeUpPath";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\mytestfile.cs", "basil was here");
            var sut = new MockFileStructureChecker(mps);

            sut.AssignFileMustNotContainAction(DUMMYRULE, " **/*.cs", "basil");

            var cr = sut.Check();
            Assert.Equal(1, cr.DefectCount);
        }
    }
}