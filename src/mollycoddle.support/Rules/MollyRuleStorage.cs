namespace mollycoddle {

    public class MollyRuleStorage {

        public MollyRuleStorage() {
            Rules = new MollyRuleStorageInstance[0];
            RulesetName = string.Empty;
        }

        public MollyRuleStorageInstance[] Rules { get; set; }
        public string RulesetName { get; set; }
    }
}