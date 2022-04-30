namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MollyRule {
        protected List<ValidatorBase> vals;
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }

        public ValidatorBase[] Validators { 
            get {
                return vals.ToArray();
            }
        }

        public MollyRule() {
            Identifier = Name = Link = string.Empty;
            vals = new List<ValidatorBase>();
        }

        internal void AddValidator(ValidatorBase vb) {
            vals.Add(vb);
        }
    }
}
