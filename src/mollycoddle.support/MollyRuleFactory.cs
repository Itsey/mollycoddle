namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MollyRuleFactory {
        public MollyRuleFactory() { }

        public IEnumerable<MollyRule> GetRules() {
            var rl =  new MollyRule() {
                Identifier = "M-0001",
                Name = "Root must contain one folder called src.",
                Link = "http://xxxx",
            };

            var dv = new DirectoryValidationChecks(rl.Identifier);

            dv.MustExist("%ROOT%\\src");
            dv.MustNotExist("%ROOT%\\src\\src");
            rl.AddValidator(dv);

            yield return rl;


            rl = new MollyRule() {
                Identifier = "M-0002",
                Name = "Root must only contain known foldernames",
                Link = "http://yyyyy"
            };

            dv = new DirectoryValidationChecks(rl.Identifier);
            dv.AddProhibitedPattern(@"%ROOT%\*", @"%ROOT%\src", @"%ROOT%\build", @"%ROOT%\doc", @"%ROOT%\res");
            rl.AddValidator(dv);

            yield return rl;
        }
    }
}
