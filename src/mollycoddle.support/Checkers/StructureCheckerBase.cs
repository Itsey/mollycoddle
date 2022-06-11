using Minimatch;
using Plisky.Diagnostics;

namespace mollycoddle {

    public abstract class StructureCheckerBase {
        protected Bilge b = new Bilge("molly-structurecheck");
        protected MollyOptions mo;
        protected Options o = new Options() { AllowWindowsPaths = true, IgnoreCase = true };
        protected List<Tuple<string, Func<string, bool>>> prohibitors = new List<Tuple<string, Func<string, bool>>>();
        protected ProjectStructure ps;
        protected List<ValidatorBase> validators = new List<ValidatorBase>();
        protected List<Func<string,bool>> bypassMatch = new List<Func<string, bool>>();

        public StructureCheckerBase(ProjectStructure ps, MollyOptions mopts) {
            this.ps = ps;
            mo = mopts;
        }

        public void AddMasterByPass(string v) {
            b.Verbose.Log($"Master Bypass {v}");
            bypassMatch.Add(Minimatcher.CreateFilter(v, o));
        }

        public void AddRuleRequirement(ValidatorBase n) {
            validators.Add(n);
        }

        public CheckResult Check() {
            return Check(new CheckResult());
        }

        public CheckResult Check(CheckResult results) {
            PrepareValidators();
            return ActualExecuteChecks(results);
        }

        protected abstract CheckResult ActualExecuteChecks(CheckResult results);

        protected virtual void AddDirectoryValidator(DirectoryValidationChecks dc) {
        }

        protected virtual void AddFileValidator(FileValidationChecks fs) {
        }

        protected virtual void AddNugetValidator(NugetValidationChecks nu) {
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

        protected void PrepareValidators() {
            b.Verbose.Log("Validator preparation occurs");

            foreach (var f in validators) {
                if (f is DirectoryValidationChecks dc) {
                    b.Verbose.Log($"Directory Validator Load {dc.TriggeringRule}");
                    AddDirectoryValidator(dc);
                }
                if (f is FileValidationChecks fs) {
                    b.Verbose.Log($"FileSystem Validator Load {fs.TriggeringRule}");
                    AddFileValidator(fs);
                }
                if (f is NugetValidationChecks nu) {
                    b.Verbose.Log($"Nuget Validator Load {nu.TriggeringRule}");
                    AddNugetValidator(nu);
                }
            }
        }

        protected string ReplaceRoot(string prohibited) {
            if (ps == null) {
                throw new InvalidOperationException("The root must be set in the validator prior to calling ReplaceRoot");
            }
            return prohibited.Replace("%ROOT%", ps.Root);
        }
    }
}