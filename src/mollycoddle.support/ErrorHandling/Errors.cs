using Plisky.Diagnostics;

namespace mollycoddle;

public enum ErrorModule : short {
    Unknown = 0x0001,
    MollyCommandLine = 0x0010,
    MollyRules = 0x0011,
    MollyOptions = 0x0012,
    NexusModule = 0x0103,
}

public enum ErrorCode : short {
    Unknown = 0x0000,
    NexusMarkerNotFound = 0x0101,
}

public static class MollyError {
    public static Bilge b = new Bilge("mollycoddle.errors");

    public static void Throw(ErrorModule module, ErrorCode code, string message) {
        b.Error.Report((short)module, (short)code, message);

        throw new Exception($"Error in {module} with code {code}: {message}");
    }
}