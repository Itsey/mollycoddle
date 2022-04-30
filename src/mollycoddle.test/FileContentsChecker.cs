namespace mollycoddle.test {
    using System;

    internal class TestFileContentsChecker: FileContentsChecker {
        protected MockProjectStructure mps;

        protected override string[] GetFileContents(string path) {
            return base.GetFileContents(path);
        }

        public TestFileContentsChecker(MockProjectStructure ps):base(ps) {
            mps = ps;
        }

    }
}