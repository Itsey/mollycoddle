namespace mollycoddle {

    public class ValidatorBase {

        public ValidatorBase(string trigger) {
            TriggeringRule = trigger;
        }

        public string TriggeringRule { get; set; }
    }
}