namespace mollycoddle.test {

    using System;

    internal class MockFileStructureChecker : FileStructureChecker {
        protected MockProjectStructure mps;

        public MockFileStructureChecker(MockProjectStructure ps) : base(ps, new MollyOptions()) {
            mps = ps;
        }

        protected override Action<MinMatchActionChecker, string> GetContentsCheckerAction(string masterContentsPath) {
            return new Action<MinMatchActionChecker, string>((fca, fn) => {
                string f = GetFileContents(masterContentsPath);
                string z = GetFileContents(fn);
                if (f != z) {
                    fca.IsInViolation = true;
                    fca.AdditionalInfo = fn;
                }
            });
        }

        protected override string? GetFileContents(string path) {
            // Dont hit the disk, isntead return mock file contents.
            if (mps.AllFiles.Contains(path)) {
                return mps.testFileContents[path];
            } else {
                return null;
            }
        }

        protected override Action<MinMatchActionChecker, string> GetFileExistChecker(bool shouldExist = true)  {
            return new Action<MinMatchActionChecker, string>((fca, fn) => {

                if (mps.AllFiles.Contains(fn) == shouldExist) {
                    fca.Passed = true;
                } else {
                    fca.IsInViolation = true;
                    fca.AdditionalInfo = fn;
                }

            });
        }

        protected override string ValidateMasterPath(string pathToMaster) {
            // Remove all validation which would hit the disk.
            return pathToMaster;
        }

        
    }
}