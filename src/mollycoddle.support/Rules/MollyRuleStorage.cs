namespace mollycoddle {


    public class MollyRuleStorage {
        public MollyRuleStorageInstance[] Rules { get; set; }
        public string RulesetName { get; set; }

        public MollyRuleStorage() {
            Rules = new MollyRuleStorageInstance[0];
            RulesetName = string.Empty;
        }
    }
}