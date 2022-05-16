﻿namespace mollycoddle {

    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// The job here is to describe the validations technically so that the checker can execute them but this class should not
    /// know how to do the validations just what the validations must be.
    /// </summary>
    [DebuggerDisplay("Validator For {TriggeringRule}")]
    public class DirectoryValidationChecks : ValidatorBase {
        private List<string> mustExistPaths = new List<string>();
        
        private List<ProhibitedPathSet> prohibittions = new List<ProhibitedPathSet>();

        public DirectoryValidationChecks(string owningRuleName) : base(owningRuleName) {
        }

        public void AddProhibitedPattern(string prohibited, params string[] exceptions) {
            prohibittions.Add(new ProhibitedPathSet(prohibited) {
                ExceptionsList = exceptions
            });
        }

        public IEnumerable<ProhibitedPathSet> GetProhibitedPaths() {
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