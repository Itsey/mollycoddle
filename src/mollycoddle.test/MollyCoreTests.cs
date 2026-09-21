namespace mollycoddle.test;

using System;
using Shouldly;
using Xunit;

public class MollyCoreTests {
    [Fact]
    public void ExecuteAllChecks_ThrowsWhenProjectStructureNotLoaded() {
        var sut = new Molly(new MollyOptions());

        Should.Throw<InvalidOperationException>(() => sut.ExecuteAllChecks());
    }

    [Fact]
    public void ImportRules_ThrowsWhenProjectStructureNotLoaded() {
        var sut = new Molly(new MollyOptions());
        var rule = new MollyRule {
            Name = "rule-one"
        };

        Should.Throw<InvalidOperationException>(() => sut.ImportRules([rule]));
    }

    [Fact]
    public void ImportRules_LoadsSupportingInfoWithFallbackMessage() {
        var mps = MockProjectStructure.Get().WithRoot(@"C:\madeup");
        var sut = new Molly(new MollyOptions());
        sut.AddProjectStructure(mps);

        var withLink = new MollyRule {
            Name = "rule-with-link",
            Link = "https://example.com/rule"
        };
        var withoutLink = new MollyRule {
            Name = "rule-without-link",
            Link = string.Empty
        };

        sut.ImportRules([withLink, withoutLink]);

        sut.GetRuleSupportingInfo("rule-with-link").ShouldBe("See: https://example.com/rule");
        sut.GetRuleSupportingInfo("rule-without-link").ShouldBe("Sorry, no further information provided.");
        sut.GetRuleSupportingInfo("missing-rule").ShouldBe(string.Empty);
    }

    [Fact]
    public void ApplyMollyFix_ReturnsNullWhenNoFixableViolations() {
        var mps = MockProjectStructure.Get().WithRoot(@"C:\madeup");
        var sut = new Molly(new MollyOptions());
        sut.AddProjectStructure(mps);

        var result = sut.ApplyMollyFix();

        result.ShouldBeNull();
    }
}
