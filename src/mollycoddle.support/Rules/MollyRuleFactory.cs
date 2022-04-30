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
                Identifier = "M0001",
                Name = "Root must contain one folder called src.",
                Link = "http://xxxx",
            };

            var dv = new DirectoryValidationChecks(rl.Identifier);

            dv.MustExist("%ROOT%\\src");
            dv.MustNotExist("%ROOT%\\src\\src");
            rl.AddValidator(dv);

            yield return rl;


            rl = new MollyRule() {
                Identifier = "M0002",
                Name = "Root must only contain known foldernames",
                Link = "http://yyyyy"
            };

            dv = new DirectoryValidationChecks(rl.Identifier);
            dv.AddProhibitedPattern(@"%ROOT%\*", @"%ROOT%\src", @"%ROOT%\build", @"%ROOT%\test", @"%ROOT%\doc", @"%ROOT%\res");
            rl.AddValidator(dv);

            yield return rl;

            rl = new MollyRule() {
                Identifier = "M0030",
                Name = "Master editorconfig file must be used",
                Link = "http://yyyyy"
            };


            var fv = new FileValidationChecks(rl.Identifier);
            fv.MustMatchMaster(@"%ROOT%\src\.editorconfig",@"%MASTERROOT%\master.editorconfig");            
            rl.AddValidator(fv);

            yield return rl;

            rl = new MollyRule() {
                Identifier = "M0050",
                Name = "Master gitignore file must be used",
                Link = "http://yyyyy"
            };


            fv = new FileValidationChecks(rl.Identifier);
            fv.MustMatchMaster(@"%ROOT%\.gitignore", @"%MASTERROOT%\master.gitignore");
            rl.AddValidator(fv);

            yield return rl;



            rl = new MollyRule() {
                Identifier = "M0010",
                Name = "Supply A Readme File",
                Link = "http://yyyyy"
            };


            fv = new FileValidationChecks(rl.Identifier);
            fv.MustExist(@"%ROOT%\readme.md");
            rl.AddValidator(fv);

            yield return rl;



            rl = new MollyRule() {
                Identifier = "M-0006",
                Name = "Put the solution under src",
                Link = "http://yyyyy"
            };


            fv = new FileValidationChecks(rl.Identifier);
            fv.MustExist(@"%ROOT%\src\*.sln");
            rl.AddValidator(fv);

            yield return rl;
        }
    }
}
