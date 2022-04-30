namespace mollycoddle.test {
    using System.IO;
    using Plisky.Diagnostics;
    using Xunit;

    public class Exploratory {
        Bilge b = new Bilge();


      


        [Fact]
        public void Exploratory2() {
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






        [Fact]
        public void Exploratory3() {
            string root = @"C:\temp\source";

            var fpc = new FilePresenceChecker();


            string rootPath = @"C:\temp\source";
            string[] dirs = { rootPath, @"C:\temp\source\project1\project1.csproj", @"C:\temp\docs\.gitignore", @"c:\temp\source\.gitignore" };
            var cr = fpc.Check(rootPath, dirs);
            Assert.Equal(0, cr.DefectCount);
        }


    }

}