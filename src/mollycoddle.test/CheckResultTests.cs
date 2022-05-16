namespace mollycoddle.test {

    using Xunit;

    public class CheckResultTests {

        [Fact]
        public void Violations_HaveRulenames() {
            var mps = MockProjectStructure.Get().WithRoot(@"c:\MadeUpPath");

            var dst = new DirectoryStructureChecker(mps, new MollyOptions());
            var dv = new DirectoryValidationChecks(MockProjectStructure.DUMMYRULENAME);
            dv.MustExist("%ROOT%\\mustexistfolder");
            dst.AddRuleRequirement(dv);

            var sut = dst.Check();

            Assert.Equal(MockProjectStructure.DUMMYRULENAME, sut.ViolationsFound[0].RuleName);
        }
    }
}