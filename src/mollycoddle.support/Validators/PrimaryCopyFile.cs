namespace mollycoddle; 

public class PrimaryCopyFile {

    public PrimaryCopyFile(string pathToMatch, string masterToMatch) {
        PatternForSourceFile = pathToMatch;
        FullPathForMasterFile = masterToMatch;
    }

    public string FullPathForMasterFile { get; set; }
    public string PatternForSourceFile { get; set; }
}