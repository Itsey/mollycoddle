namespace mollycoddle {

    public class MollyRuleStepStorage {
        public string[] AdditionalData { get; set; }
        public string Control { get; set; }
        public string PatternMatch { get; set; }
        public string ValidatorName { get; set; }
    }

    public class MollyRuleStorage {
        public MollyRuleStorageInstance[] Rules { get; set; }
        public string RulesetName { get; set; }
    }

    public class MollyRuleStorageInstance {
        public string Link { get; set; }
        public string Name { get; set; }
        public string RuleReference { get; set; }
        public MollyRuleStepStorage[] Validators { get; set; }
    }
}