namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// The job here is to describe the validations technically so that the checker can execute them but this class should not
    /// know how to do the validations just what the validations must be.
    /// </summary>
    [DebuggerDisplay("Validator For {TriggeringRule}")]
    public class FileValidationChecks : ValidatorBase {
        private List<MasterSlaveFile> masterMatchers = new List<MasterSlaveFile>();
        private List<string> mustExistPaths = new List<string>();
        private List<MatchWithSecondaryMatches> prohibittions = new List<MatchWithSecondaryMatches>();
        private List<MatchWithSecondaryMatches> precisePositions = new List<MatchWithSecondaryMatches>();

        public FileValidationChecks(string owningRuleName) : base(owningRuleName) {
        }

        public void AddProhibitedPattern(string prohibited, params string[] exceptions) {
            prohibittions.Add(new MatchWithSecondaryMatches(prohibited) {
                SecondaryList = exceptions
            });
        }

        public IEnumerable<string> FilesThatMustExist() {
            return mustExistPaths;
        }

        public IEnumerable<MasterSlaveFile> FilesThatMustMatchTheirMaster() {
            return masterMatchers;
        }

        public IEnumerable<MatchWithSecondaryMatches> FilesThatMustNotExist() {
            return prohibittions;
        }

        public IEnumerable<MatchWithSecondaryMatches> FilesThatIfTheyDoExistMustBeInTheRightPlace() {
            return precisePositions;
        }


        public void MustExist(string v) {
            mustExistPaths.Add(v);
        }

        public void MustMatchMaster(string pathToMatch, string masterToMatch) {
            mustExistPaths.Add(pathToMatch);
            masterMatchers.Add(new MasterSlaveFile(pathToMatch, masterToMatch));
        }
 

        internal void MustBeInSpecificLocation(string patternMatch, string[] additionalData) {
            precisePositions.Add(new MatchWithSecondaryMatches(patternMatch) {
                SecondaryList = additionalData
            });
        }
    }
}