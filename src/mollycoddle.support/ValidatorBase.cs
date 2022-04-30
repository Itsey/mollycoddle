namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ValidatorBase {
        public string TriggeringRule { get; set; }
        

        public ValidatorBase(string trigger) {
            TriggeringRule = trigger;            
        }
    }
}
