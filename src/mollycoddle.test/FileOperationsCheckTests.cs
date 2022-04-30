using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plisky.Diagnostics;
using Xunit;

namespace mollycoddle.test {
    public class FileOperationsCheckTests {
        protected Bilge b = new Bilge();

        [Fact]
        public void FileMustExist_PassesIfFileExists() {
            b.Info.Flow();

            string root = @"C:\temp\source";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\mytestfile.cs", "basil was here");
            var sut = new MockFileStructureChecker(mps);
            sut.AssignMustExistAction("dummyrule", "**/*.cs");

            var cr = sut.Check();

            Assert.Equal(0, cr.DefectCount);
        }

        [Fact]
        public void FileMustExist_FailsIfFileDoesNotExist() {
            b.Info.Flow();

            string root = @"C:\temp\source";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\mytestfile.bob", "basil was here");
            var sut = new MockFileStructureChecker(mps);
            sut.AssignMustExistAction("dummyrule", "**/*.cs");

            var cr = sut.Check();

            Assert.Equal(1, cr.DefectCount);

        }


        [Fact(DisplayName = nameof(CompareWithMasterFile_RuleWorks))]
        public void CompareWithMasterFile_RuleWorks() {
            b.Info.Flow();

            string root = @"C:\temp\source";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\.gitignore", "gitignorefilecontents");
            mps.WithFile("%MASTEROOT%\\master.gitignore", "gitignorefilecontents");

            var sut = new MockFileStructureChecker(mps);
            sut.AssignCompareWithMasterAction("**/.gitignore", "%MASTEROOT%\\master.gitignore", "DummyRule");

            var cr = sut.Check();

            Assert.Equal(0, cr.DefectCount);
        }

        [Fact(DisplayName = nameof(CompareWithMasterFile_RuleWorks))]
        public void FileMustNotContain_RuleWorks() {
            b.Info.Flow();

            string root = @"C:\temp\source";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\mytestfile.cs", "basil was here");
            var sut = new MockFileStructureChecker(mps);

            sut.AssignFileMustNotContainAction("dummyrule", "**/*.cs", "basil");

            var cr = sut.Check();
            Assert.Equal(1, cr.DefectCount);
        }


    }
}
