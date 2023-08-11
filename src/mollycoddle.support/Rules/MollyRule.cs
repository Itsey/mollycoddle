namespace mollycoddle {

    using System.Collections.Generic;

    public class MollyRule {
        protected List<ValidatorBase> vals;

        public MollyRule() {
            Identifier = Name = Link = string.Empty;
            vals = new List<ValidatorBase>();
        }

        public string Identifier { get; set; }
        public string? Link { get; set; }
        public string Name { get; set; }

        public ValidatorBase[] Validators {
            get {
                return vals.ToArray();
            }
        }

        internal void AddValidator(ValidatorBase vb) {
            vals.Add(vb);
        }
    }
}