namespace mollycoddle;

public enum MollyErrorCode : short {
    ProgramCommandLineInvalidCommonDirectory = 0x0001,
    ProgramCommandLineRulesFileMissing = 0x0002,
    MustMatchRuleMissingMatchConditions = 0x3
}

public enum MollySubSystem : short {
    Unknown = 0x0001,
    Program = 0x0002,
    RulesFiles = 0x3
}

public class MollyErrorHandling {
}