namespace mollycoddle {

    public class ProhibitedPathSet {

        public ProhibitedPathSet(string ptn) {
            ExceptionsList = new string[0];
            ProhibitedPattern = ptn;
        }

        public string[] ExceptionsList { get; internal set; }
        public string ProhibitedPattern { get; internal set; }
    }
}