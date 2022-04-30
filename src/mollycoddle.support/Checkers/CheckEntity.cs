namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a check of some activity.  By default the rule will say that it is passed - for rules that should be false until some condition
    /// is met then ensure to set Passed = false on creation.
    /// </summary>
    public class CheckEntity {
        

        public CheckEntity(string triggeringRule) {
            OwningRuleIdentity = triggeringRule;
            Passed = true;
        }

        public string OwningRuleIdentity { get; set; }
        public string Value { get; set; }
     
        /// <summary>
        /// Passed is used for rules where it must actively pass in order to determine if its in violaton or not. Defaults to true.
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// IsInViolation actually indicates if the violation has occured.
        /// </summary>
        public bool IsInViolation { get; set; }


        
     
    }
}
