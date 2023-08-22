namespace mollycoddle.test;

using Plisky.Test;
using Xunit;

public class CheckResultTests {

    [Fact]
    [Build(BuildType.Any)]
    [Integration]
    public void Violations_HaveRulenames() {
        var mps = MockProjectStructure.Get().WithRoot(@"c:\MadeUpPath");

        var dst = new DirectoryStructureChecker(mps, new MollyOptions());
        var dv = new DirectoryValidator(MockProjectStructure.DUMMYRULENAME);
        dv.MustExist("%ROOT%\\mustexistfolder");
        dst.AddRuleRequirement(dv);

        var sut = dst.Check();

        Assert.Equal(MockProjectStructure.DUMMYRULENAME, sut.ViolationsFound[0].RuleName);
    }
}