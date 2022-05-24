using Plisky.Plumbing;

namespace mollycoddle {

    [CommandLineArguments]
    public class MollyCommandLine {

        public MollyCommandLine() {
            
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

        [CommandLineArg("warnonly")]
        public bool WarningMode { get; set; }


        public MollyOptions GetOptions() {
            var result = new MollyOptions();
            WarningMode = false;
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