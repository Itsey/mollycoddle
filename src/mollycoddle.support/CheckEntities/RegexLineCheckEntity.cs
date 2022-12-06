﻿namespace mollycoddle {

    using System;
    using Minimatch;

    /// <summary>
    /// Implements the regexvalidatiors as a check entity. This will run through the lines of matched files and look for regex matches, or must not matches and then fail accordingly.  
    /// </summary>
    public class RegexLineCheckEntity : CheckEntityBase {


        public Action<RegexLineCheckEntity, string> PerformCheck { get; set; }

        public RegexLineCheckEntity(string owningRuleName) : base(owningRuleName) {
        }

       
        public Minimatcher DoesMatch { get; internal set; }

        public bool ExecuteCheckWasViolation(string fileName) {
            PerformCheck(this, fileName);
            return this.IsInViolation;
        }
    }
}