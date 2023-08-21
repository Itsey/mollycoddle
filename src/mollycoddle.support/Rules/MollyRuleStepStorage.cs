namespace mollycoddle {

    public class MollyRuleStepStorage {

        public MollyRuleStepStorage() {
            AdditionalData = new string[0];
            Control = PatternMatch = ValidatorName = string.Empty;
        }

        public string[] AdditionalData { get; set; }
        public string Control { get; set; }
        public string PatternMatch { get; set; }
        public string ValidatorName { get; set; }
    }
}