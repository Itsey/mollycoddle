namespace mollycoddle {

    /// <summary>
    /// Holds a match pattern and a set of secondary patterns, for example a match and a list of exceptions.
    /// </summary>
    public class MatchWithSecondaryMatches {

        public MatchWithSecondaryMatches(string ptn) {
            SecondaryList = new string[0];
            PrimaryPattern = ptn;
        }

        /// <summary>
        /// One or more secondary patterns
        /// </summary>
        public string[] SecondaryList { get; set; }

        /// <summary>
        /// The primary matching pattern
        /// </summary>
        public string PrimaryPattern { get; set; }
    }
}