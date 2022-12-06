using System.Text.RegularExpressions;

namespace mollycoddle.test {
    public class MockRegexLineCheckEntity : RegexLineCheckEntity {
        public MockRegexLineCheckEntity(string dummyRuleName) : base(dummyRuleName) {
        }

        public Regex RegularExpression { get; set; }

     
    }
}