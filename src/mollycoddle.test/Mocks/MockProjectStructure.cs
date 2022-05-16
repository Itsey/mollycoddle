namespace mollycoddle.test {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal class MockProjectStructure : ProjectStructure {
        public static string DUMMYRULENAME = "testrule";
        public Dictionary<string, string> testFileContents = new Dictionary<string, string>();

        public override string GetFileContents(string filename) {
            if (this.AllFiles.Contains(filename)) {
                return testFileContents[filename];
            }
            return null;
        }

        public override bool DoesFileExist(string filename) {
            
            return this.AllFiles.Contains(filename);
        }

        public override Tuple<long, byte[]> GetFileHashAndLength(string masterContentsPath) {
            string cnts = testFileContents[masterContentsPath];
            return new Tuple<long, byte[]>(cnts.Length,Encoding.UTF8.GetBytes(cnts));
        }

        internal static MockProjectStructure Get() {
            return new MockProjectStructure();
        }

        internal void WithFile(string fullFileName, string fileContents) {
            AllFiles.Add(fullFileName);
            testFileContents.Add(fullFileName, fileContents);
        }

        internal MockProjectStructure WithFolder(params string[] folders) {
            foreach (var nextPath in folders) {
                base.AllFolders.Add(nextPath);
            }
            return this;
        }

        internal MockProjectStructure WithRoot(string v) {
            base.Root = v;
            return this;
        }

        internal void WithRootedFile(string fileName, string fileContents = "test") {
            if (fileName.Contains("%ROOT%")) {
                fileName = fileName.Replace("%ROOT%", base.Root);
            } else {
                fileName = Path.Combine(base.Root, fileName);
            }
            AllFiles.Add(fileName);
            testFileContents.Add(fileName, fileContents);
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
    }
}