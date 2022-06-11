namespace mollycoddle {

    /// <summary>
    /// MollyOptions are cross cutting options relating to how the program works.
    /// </summary>
    public class MollyOptions {

        public MollyOptions() {
            DirectoryToTarget = RulesFile = string.Empty;
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
        /// The rulesfile that is to be loaded, either a rules set or a single rules file.
        /// </summary>
        public string RulesFile { get; set; }
    }
}