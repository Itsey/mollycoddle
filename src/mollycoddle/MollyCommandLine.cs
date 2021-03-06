namespace mollycoddle {
    using Plisky.Plumbing;

    [CommandLineArguments]
    public class MollyCommandLine {

        public MollyCommandLine() {
            
        }

        [CommandLineArg("disabled", FullDescription = "If Disabled is set the mollycoddle will not execute but will return 0 instead.")]
        public bool Disabled { get; set; }

        [CommandLineArg("dir", IsRequired = true, IsSingleParameterDefault = true, Description ="The root to start scanning in")]
        public string DirectoryToTarget { get; set; }

        [CommandLineArg("masterRoot", FullDescription = "If Master files based rules are used this is the location of the master Root for these files")]
        public string MasterPath { get; set; }

        [CommandLineArg("rulesfile", FullDescription = "Path to either a mollyset or molly rules file")]
        public string RulesFile { get; set; }

        [CommandLineArg("formatter", FullDescription = "If set to azdo then azure build pipelines formatting will be used, otherwise plain text output")]
        public string OutputFormat { get; set; }

        [CommandLineArg("warnonly", FullDescription = "If set then MollyCoddle will return zero even if faults are found, but the faults will be outputted")]
        public bool WarningMode { get; set; }


        public MollyOptions GetOptions() {
            var result = new MollyOptions();
            WarningMode = false;
            result.MasterPath = MasterPath;
            if (!string.IsNullOrWhiteSpace(DirectoryToTarget)) {
                if (DirectoryToTarget.EndsWith("\\")) {
                    result.DirectoryToTarget = DirectoryToTarget.Substring(0, DirectoryToTarget.Length - 1);
                } else {
                    result.DirectoryToTarget = DirectoryToTarget;
                }
            }
            OutputFormat = "default";
            result.RulesFile = RulesFile;
            return result;
        }
    }
}