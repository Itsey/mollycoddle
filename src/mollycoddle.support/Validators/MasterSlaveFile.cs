namespace mollycoddle {

    public class MasterSlaveFile {

        public MasterSlaveFile(string pathToMatch, string masterToMatch) {
            PatternForSourceFile = pathToMatch;
            FullPathForMasterFile = masterToMatch;
        }

        public string FullPathForMasterFile { get; set; }
        public string PatternForSourceFile { get; set; }
    }
}