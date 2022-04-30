namespace mollycoddle.test {
    using System.IO;
    using Plisky.Diagnostics;
    using Xunit;

    public class Exploratory {
        Bilge b = new Bilge();


        [Fact(DisplayName = nameof(Exploratory1))]
        public void Exploratory1() {
            b.Info.Flow();

            string root = @"C:\temp\source";
            var mps = MockProjectStructure.Get().WithRoot(root);            
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\.gitignore", "gitignorefilecontents");
            mps.WithFile("%MASTERPATH%\\master.gitignore", "gitignorefilecontents");
            var sut = new TestFileContentsChecker(mps);            
            sut.CompareWithMaster("**/.gitignore","%MASTERPATH%\\master.gitignore");

            var cr = sut.CheckFiles();

            Assert.Equal(0, cr.DefectCount);
        }

        [Fact(DisplayName = nameof(Exploratory1))]
        public void Exploratory2() {
            b.Info.Flow();

            string root = @"C:\temp\source";
            var mps = MockProjectStructure.Get().WithRoot(root);            
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\mytestfile.cs", "basil was here");
            var sut = new TestFileContentsChecker(mps);
            
            sut.MustNotContain("**/.cs", "basil");

            var cr = sut.CheckFiles();
            Assert.Equal(3, cr.DefectCount);
        }



        [Fact]
        public void Exploratory3() {
            string root = @"C:\temp\source";

            FilePresenceChecker fpc = new FilePresenceChecker();
            

            string rootPath = @"C:\temp\source";
            string[] dirs = { rootPath, @"C:\temp\source\project1\project1.csproj", @"C:\temp\docs\.gitignore", @"c:\temp\source\.gitignore" };
            var cr = fpc.Check(rootPath, dirs);
            Assert.Equal(0, cr.DefectCount);
        }


    }

}