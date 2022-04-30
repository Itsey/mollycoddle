namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Minimatch;


    /// <summary>
    /// The job here is to describe the validations technically so that the checker can execute them but this class should not
    /// know how to do the validations just what the validations must be.
    /// </summary>
    [DebuggerDisplay("Validator For {TriggeringRule}")]
    public class FileValidationChecks : ValidatorBase {

        List<string> mustExistPaths = new List<string>();
        List<string> mustNotExistPaths = new List<string>();
        List<ProhibitedPathSet> prohibittions = new List<ProhibitedPathSet>();
        List<MasterSlaveFile> masterMatchers = new List<MasterSlaveFile>();

        public FileValidationChecks(string owningRuleName) : base(owningRuleName) {
            
        }

        public void MustExist(string v) {
            mustExistPaths.Add(v);
        }

        public void MustNotExist(string v) {
            mustNotExistPaths.Add(v);
        }

        public IEnumerable<string> FilesThatMustExist() {
            return mustExistPaths;
        }

        public IEnumerable<ProhibitedPathSet> FilesThatMustNotExist() {
            return prohibittions;
        }

        public IEnumerable<MasterSlaveFile> FilesThatMustMatchTheirMaster() {
            return masterMatchers;
        }

        public void MustMatchMaster(string pathToMatch, string masterToMatch) {
            mustExistPaths.Add(pathToMatch);
            masterMatchers.Add(new MasterSlaveFile(pathToMatch, masterToMatch));
        }

        public void AddProhibitedPattern(string prohibited, params string[] exceptions) {
            prohibittions.Add(new ProhibitedPathSet(prohibited) {                
                ExceptionsList = exceptions
            });
        }


        public IEnumerable<string> MustNotExistExactly() {
            return mustNotExistPaths;
        }
    }
}