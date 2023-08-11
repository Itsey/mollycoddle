namespace mollycoddle {

    /// <summary>
    /// MollyOptions are cross cutting options relating to how the program works.
    /// </summary>
    public class MollyOptions {

        public MollyOptions() {
            DebugSetting = DirectoryToTarget = RulesFile = string.Empty;            
        }

        /// <summary>
        /// The directory against which the analysis is run.
        /// </summary>
        public string DirectoryToTarget { get; set; }
        /// <summary>        
        /// The Path to the master files for any master comparisons that need to be made
        /// </summary>
        public string? MasterPath { get; set; }

        /// <summary>
        /// The rules file that is to be loaded, either a rules set or a single rules file.
        /// </summary>
        public string RulesFile { get; set; }

        /// <summary>
        /// Decides if links are written out alongside the list of violations
        /// </summary>
        public bool AddHelpText { get; set; }

        /// <summary>
        /// Determines whether debugging should be enabled.
        /// </summary>
        public bool EnableDebug { get; set; }

        /// <summary>
        /// Specifies the level of trace to be enabled as a Plisky.Diagnostics debug string.
        /// </summary>
        public string DebugSetting { get; set; }
    }
}