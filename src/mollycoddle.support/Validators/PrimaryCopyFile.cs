namespace mollycoddle;

public class PrimaryCopyFile {

    public PrimaryCopyFile(string pathToMatch, string masterToMatch) {
        PatternForSourceFile = pathToMatch;
        FullPathForCommonFile = masterToMatch;
    }

    public string FullPathForCommonFile { get; set; }
    public string PatternForSourceFile { get; set; }
}