using Minimatch;
using Plisky.Diagnostics;

namespace mollycoddle {
    public abstract class StructureCheckerBase {
        protected Bilge b = new Bilge("molly-structurecheck");
        protected Options o = new Options() { AllowWindowsPaths = true, IgnoreCase = true };
        protected MollyOptions mo;
        protected ProjectStructure ps;
        protected List<ValidatorBase> validators = new List<ValidatorBase>();
        protected List<Tuple<string, Func<string, bool>>> prohibitors = new List<Tuple<string, Func<string, bool>>>();
      

        public StructureCheckerBase(ProjectStructure ps, MollyOptions mopts) {
            this.ps = ps;
            mo = mopts;
        }

        protected string ReplaceRoot(string prohibited) {
            if (ps == null) {
                throw new InvalidOperationException("The root must be set in the validator prior to calling ReplaceRoot");
            }
            return prohibited.Replace("%ROOT%", ps.Root);
        }


        protected void AddProhibitedPatternFinder(string ruleName, string prohibited, params string[] exceptions) {
            prohibited = ReplaceRoot(prohibited);
            b.Verbose.Log($"Creating prohibition {ruleName}");

            var flt = Minimatcher.CreateFilter(prohibited, o);
            List<Minimatcher> exceptFor = new List<Minimatcher>();
            foreach (string exception in exceptions) {
                var ne = ReplaceRoot(exception);
                var m = new Minimatcher(ne, o);
                exceptFor.Add(m);
            }
            Func<string, bool> isProhibited = (pat) => {
                if (flt(pat)) {
                    foreach (var n in exceptFor) {
                        if (n.IsMatch(pat)) {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            };
            prohibitors.Add(new Tuple<string, Func<string, bool>>(ruleName, isProhibited));

        }

        public CheckResult Check() {
            return Check(new CheckResult());
        }

        public CheckResult Check(CheckResult results) {
            PrepareValidators();
            return ActualExecuteChecks(results);
        }

        protected void PrepareValidators() {
            foreach (var f in validators) {
                if (f is DirectoryValidationChecks dc) {
                    AddDirectoryValidator(dc);
                }
                if (f is FileValidationChecks fs) {
                    AddFileValidator(fs);
                }
            }
        }

        protected virtual void AddFileValidator(FileValidationChecks fs) {
            
        }

        protected virtual void AddDirectoryValidator(DirectoryValidationChecks dc) {
            
        }

        protected abstract CheckResult ActualExecuteChecks(CheckResult results);

        public void AddRuleRequirement(ValidatorBase n) {
            validators.Add(n);
        }
    }
}