namespace mollycoddle.test {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class MockProjectStructure : ProjectStructure {
        public static string DUMMYRULENAME = "testrule";
        public Dictionary<string, string> testFileContents = new Dictionary<string, string>();

        internal MockProjectStructure WithRoot(string v) {
            base.Root = v;
            return this;
        }

        internal void WithFile(string fullFileName, string fileContents) {
            AllFiles.Add(fullFileName);
            testFileContents.Add(fullFileName,fileContents);
        }



        internal MockProjectStructure WithRootedFolder(params string[] rootedFolders) {
            foreach (var tp in rootedFolders) {
                var nextPath = tp;
                if (nextPath.Contains("%ROOT%")) {
                    nextPath = nextPath.Replace("%ROOT%", base.Root);
                } else {
                    nextPath = Path.Combine(base.Root, nextPath);
                }
                base.AllFolders.Add(nextPath);
            }
            return this;
        }

        internal void WithRootedFile(string fileName, string fileContents ="test") {

            if (fileName.Contains("%ROOT%")) {
                fileName = fileName.Replace("%ROOT%", base.Root);
            } else {
                fileName = Path.Combine(base.Root, fileName);
            }
            AllFiles.Add(fileName);            
            testFileContents.Add(fileName , fileContents);
        }

        internal static MockProjectStructure Get() {
            return new MockProjectStructure();
        }

        internal MockProjectStructure WithFolder(params string[] folders) {
            foreach (var nextPath in folders) {
                base.AllFolders.Add(nextPath);
            }
            return this;
        }
    }
}
