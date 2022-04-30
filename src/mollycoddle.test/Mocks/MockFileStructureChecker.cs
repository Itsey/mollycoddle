namespace mollycoddle.test {
    using System;

    internal class MockFileStructureChecker: FileStructureChecker {
        protected MockProjectStructure mps;

        protected override string ValidateMasterPath(string pathToMaster) {
            // Remove all validation which would hit the disk.
            return pathToMaster;
        }

        protected override string GetFileContents(string path) {
            // Dont hit the disk, isntead return mock file contents.
            if (mps.AllFiles.Contains(path)) {
                return  mps.testFileContents[path] ;
            } else {
                return null;
            }
        }

        protected override Action<FileCheckEntity, string> GetContentsCheckerAction(string masterContentsPath) {
            return new Action<FileCheckEntity, string>((fca,fn) => {
                var f = GetFileContents(masterContentsPath);
                var z = GetFileContents(fn);
                if (f != z) {
                    fca.IsInViolation = true;
                    fca.AdditionalInfo = fn;
                }
            });

          
        }

        protected override Action<FileCheckEntity, string> GetMustExistChecker() {
            return new Action<FileCheckEntity, string>((fca, fn) => {
                if (mps.AllFiles.Contains(fn)) {
                    fca.Passed = true;
                }
            });
        }

        public MockFileStructureChecker(MockProjectStructure ps):base(ps, new MollyOptions()) {
            mps = ps;
        }

    }
}