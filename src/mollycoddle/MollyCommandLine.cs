using Plisky.Plumbing;

namespace mollycoddle {

    [CommandLineArguments]
    public class MollyCommandLine {

        public MollyCommandLine() {
            // MasterPath = @"C:\Files\Code\git\mollycoddle\src\_Dependencies\TestMasterPath\";
        }

        [CommandLineArg("disabled")]
        public bool Disabled { get; set; }

        [CommandLineArg("dir", IsRequired = true, IsSingleParameterDefault = true)]
        public string DirectoryToTarget { get; set; }

        [CommandLineArg("masterRoot")]
        public string MasterPath { get; set; }

        [CommandLineArg("rulesfile")]
        public string RulesFile { get; set; }

        [CommandLineArg("formatter")]
        public string OutputFormat { get; set; }

        public MollyOptions GetOptions() {
            var result = new MollyOptions();
            result.MasterPath = MasterPath;
            if (DirectoryToTarget.EndsWith("\\")) {
                result.DirectoryToTarget = DirectoryToTarget.Substring(0, DirectoryToTarget.Length - 1);
            } else {
                result.DirectoryToTarget = DirectoryToTarget;
            }
            OutputFormat = "default";
            result.RulesFile = RulesFile;
            return result;
        }
    }
}