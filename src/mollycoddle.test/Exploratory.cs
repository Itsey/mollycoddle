namespace mollycoddle.test {
    using System.IO;
    using Plisky.Diagnostics;
    using Plisky.Diagnostics.Listeners;
    using Plisky.Test;
    using Xunit;

    
    public class Exploratory {
        private Bilge b = new Bilge();
        private string dummyRuleName = "drn";
        private UnitTestHelper u;

        public Exploratory() {
            Bilge.SetConfigurationResolver((a, b) => {
                if (!a.Contains("Plisky-UnitTestHelper")) {
                    return System.Diagnostics.SourceLevels.Verbose;
                }
                return b;
            });
            Bilge.AddHandler(new TCPHandler("127.0.0.1", 9060));
            b = new Bilge();
            b.Info.Log("Ello");
            u = new Plisky.Test.UnitTestHelper();
        }


        
        [Fact]
        [Fresh]
        public void Exploratory_1a() {
      

        }

        [Fact]
        [Fresh]
        public void Exploratory_1b() {
            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFolder("pickle");
            mps.WithRootedFile("src\\testproj\\testproj.csproj", "basil was here");   // Pass
            mps.WithRootedFile("src_two\\testproj2\\testproj.csproj", "basil was here");  // Pass
            mps.WithRootedFile("src\\testproj2\\nested\\testproj.csproj", "basil was here");  // Fail - nested projects
            
            var sut = new MockFileStructureChecker(mps);
            string[] secondary = new string[] { "**/src*/*/*.csproj" };
            sut.AssignIfItExistsItMustBeHereAction(dummyRuleName, new MatchWithSecondaryMatches("**/*.csproj") {  SecondaryList = secondary });
            var cr = sut.Check();

            Assert.Equal(1, cr.DefectCount);

        }

        [Fact]
        [Fresh]
        public void Exploratory_1c() {
            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFolder("pickle");
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
        [Fresh]
        public void Exploratory_OneLanguageToRuleThemAll() {
            b.Info.Flow();
      
          
            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            mps.WithRootedFile("src\\mytestfile.cs", "basil was here");
            var sut = new MockFileStructureChecker(mps);
            sut.AssignFileMustNotContainAction("dummyrule", "**/*.cs", "basil");

            var cr = sut.Check();

            Assert.Equal(1, cr.DefectCount);
        }

        [Fact]
        [Fresh]
        public void Exploratory_NubMistakes() {
            string root = @"c:\MadeUpPath";
            // .csproj.user
            // .suo
            // /bin/debug
            var fpc = new FilePresenceChecker();

            string rootPath = @"c:\MadeUpPath";
            string[] dirs = { rootPath, @"c:\MadeUpPath\project1\project1.csproj", @"C:\temp\docs\.gitignore", @"c:\MadeUpPath\.gitignore" };
            var cr = fpc.Check(rootPath, dirs);
            Assert.Equal(0, cr.DefectCount);
        }
    }
}