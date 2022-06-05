namespace mollycoddle {

    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// The job here is to describe the validations technically so that the checker can execute them but this class should not
    /// know how to do the validations just what the validations must be.
    /// </summary>
    [DebuggerDisplay("Validator For {TriggeringRule}")]
    public class DirectoryValidationChecks : ValidatorBase {
        /// <summary>
        /// This is the name of the validator, it must be specified exactly in the rules files.  It does not use nameof to prevent accidental refactoring.
        /// </summary>
        public const string VALIDATORNAME = "DirectoryValidationChecks";   
        private List<string> mustExistPaths = new List<string>();
        
        private List<MatchWithSecondaryMatches> prohibittions = new List<MatchWithSecondaryMatches>();

        public DirectoryValidationChecks(string owningRuleName) : base(owningRuleName) {
        }

        public void AddProhibitedPattern(string prohibited, params string[] exceptions) {
            prohibittions.Add(new MatchWithSecondaryMatches(prohibited) {
                SecondaryList = exceptions
            });
        }

        public IEnumerable<MatchWithSecondaryMatches> GetProhibitedPaths() {
            return prohibittions;
        }

        public void MustExist(string v) {
            mustExistPaths.Add(v);
        }

        public IEnumerable<string> MustExistExactly() {
            return mustExistPaths;
        }


    }
}