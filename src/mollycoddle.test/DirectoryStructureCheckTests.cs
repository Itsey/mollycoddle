namespace mollycoddle.test {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class DirectoryStructureCheckTests {

        [Fact]
        public void MustExistFolder_FailsIfNotFound() {
            var mps = MockProjectStructure.Get().WithRoot(@"C:\temp\source");
            mps.WithRootedFolder("project1").WithFolder(@"C:\temp\docs\");

            DirectoryStructureChecker sut = new DirectoryStructureChecker(mps);
            var dv = new DirectoryValidationChecks(MockProjectStructure.DUMMYRULENAME);
            dv.MustExist("%ROOT%\\mustexistfolder");
            sut.AddRuleRequirement(dv);

            var cr = sut.CheckDirectories();
            Assert.Equal(1, cr.DefectCount);
        }

        [Fact]
        public void MustExistFolder_NoFailIfFound() {

            var mps = MockProjectStructure.Get().WithRoot(@"C:\temp\source");
            mps.WithRootedFolder("project1").WithRootedFolder("src").WithFolder(@"C:\temp\docs\");

            var sut = new DirectoryStructureChecker(mps);

            var cr = sut.CheckDirectories();
            Assert.Equal(0, cr.DefectCount);
        }

        [Fact]
        public void ProhibitedPattern_RootReplacement_Works() {

            string root = @"C:\temp\source";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("project1");
            mps.WithRootedFolder("src");

            DirectoryStructureChecker sut = new DirectoryStructureChecker(mps);
            var dv = new DirectoryValidationChecks(MockProjectStructure.DUMMYRULENAME);
            dv.AddProhibitedPattern(@"%ROOT%\*", $"%ROOT%\\src");
            sut.AddRuleRequirement(dv);

            var cr = sut.CheckDirectories();

            Assert.Equal(1, cr.DefectCount);
            Assert.Contains("project1", cr.ViolationsFound[0].Additional);
        }

        [Fact]
        public void ProhibitedPattern_Basic_Works() {
            string root = @"C:\temp\source";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("build");
            mps.WithRootedFolder("doc");
            mps.WithRootedFolder("project1");

            DirectoryStructureChecker sut = new DirectoryStructureChecker(mps);
            var dv = new DirectoryValidationChecks(MockProjectStructure.DUMMYRULENAME);
            dv.AddProhibitedPattern($"{root}\\*");
            sut.AddRuleRequirement(dv);

            var cr = sut.CheckDirectories();
            Assert.Equal(3, cr.DefectCount);
        }

        [Fact]
        public void ProhibitedPattern_ExceptionsWork() {
            string root = @"C:\temp\source";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFolder("build");
            mps.WithRootedFolder("doc");
            mps.WithRootedFolder("project1");

            DirectoryStructureChecker sut = new DirectoryStructureChecker(mps);
            var dv = new DirectoryValidationChecks(MockProjectStructure.DUMMYRULENAME);
            dv.AddProhibitedPattern(root + @"\*", $"{root}\\src", $"{root}\\build", $"{root}\\doc");
            sut.AddRuleRequirement(dv);

            var cr = sut.CheckDirectories();

            Assert.Equal(1, cr.DefectCount);
            Assert.Contains("project1", cr.ViolationsFound[0].Additional);
        }


        [Fact]
        public void BugRepro_MustExist_Src_Works() {

            string root = @"C:\Files\Code\git\PliskyDiagnostics";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");

            DirectoryStructureChecker sut = new DirectoryStructureChecker(mps);
            var dv = new DirectoryValidationChecks(MockProjectStructure.DUMMYRULENAME);
            dv.MustExist("%ROOT%\\src");
            dv.MustNotExist("%ROOT%\\src\\src");

            sut.AddRuleRequirement(dv);

            var cr = sut.CheckDirectories();

            Assert.Equal(0, cr.DefectCount);
        }


    }
}
