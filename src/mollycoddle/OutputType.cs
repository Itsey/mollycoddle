// See https://aka.ms/new-console-template for more information

namespace mollycoddle {

    [Flags]
    public enum OutputType {
        None = 0,
        Violation = 0x0001,
        Error = 0x0002,
        Info = 0x0004,
        Verbose = 0x0008,
        EndSuccess = 0x10,
        EndFailure = 0x20
    }
}