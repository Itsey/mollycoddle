namespace mollycoddle.test;

using System.Text.RegularExpressions;

public class MockRegexLineCheckEntity : RegexLineCheckEntity {

    public MockRegexLineCheckEntity(string dummyRuleName) : base(dummyRuleName) {
        RegularExpression = new Regex(string.Empty);
    }

    public Regex RegularExpression { get; set; }
}