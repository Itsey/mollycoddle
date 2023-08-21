namespace mollycoddle {

    /// <summary>
    /// Holds the storage for a single rule, which includes a name, documentation link, reference and then a series
    /// of validators that actually implement the rule.
    /// </summary>
    public class MollyRuleStorageInstance {

        public MollyRuleStorageInstance() {
            Name = RuleReference = string.Empty;
            Validators = new MollyRuleStepStorage[0];
        }

        public string? Link { get; set; }
        public string Name { get; set; }
        public string RuleReference { get; set; }
        public MollyRuleStepStorage[] Validators { get; set; }
    }
}