namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class CheckEntity {
        

        public CheckEntity(string triggeringRule) {
            OwningRuleIdentity = triggeringRule;
        }

        public string OwningRuleIdentity { get; set; }
        public string Value { get; set; }
        public bool Passed { get; set; }
    }
}
