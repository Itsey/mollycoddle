namespace mollycoddle {

    public class NugetValidationCheck {
        
        public string Pattern { get; internal set; }
        public string[] ProhibitedPackages { get; internal set; }

        public string[] MustIncludePackages { get; internal set; }

        public NugetValidationCheck() {
            ProhibitedPackages = new string[0];
            MustIncludePackages = new string[0];
        }
    }
}