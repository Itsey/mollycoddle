namespace mollycoddle {

    using Minimatch;
    using Plisky.Diagnostics;

    public abstract class StructureCheckerBase {
        protected Bilge b = new("molly-structurecheck");
        protected List<Func<string, bool>> bypassMatch = new();
        protected MollyOptions mo;
        protected Options o = new() { AllowWindowsPaths = true, IgnoreCase = true };
        protected List<Tuple<string, Func<string, bool>>> prohibitors = new();
        protected ProjectStructure ps;
        protected List<ValidatorBase> validators = new();

        public StructureCheckerBase(ProjectStructure ps, MollyOptions mopts) {
            this.ps = ps;
            mo = mopts;
        }

        public void AddMasterByPass(string v) {
            b.Verbose.Log($"Master Bypass {v}");
            bypassMatch.Add(Minimatcher.CreateFilter(v, o));
        }

        public void AddRuleRequirement(ValidatorBase n) {
            b.Verbose.Log($"New Rule Requirement added {n.TriggeringRule}");
            validators.Add(n);
        }

        public CheckResult Check() {
            return Check(new CheckResult());
        }

        public CheckResult Check(CheckResult results) {
            b.Verbose.Log("Base Check Initialised, Preparing");
            PrepareValidators();
            b.Verbose.Log("Validators Prepared, Executing");
            return ActualExecuteChecks(results);
        }

        protected abstract CheckResult ActualExecuteChecks(CheckResult results);

        protected virtual void AddDirectoryValidator(DirectoryValidator dc) {
        }

        protected virtual void AddFileValidator(FileValidator fs) {
        }

        protected virtual void AddNugetValidator(NugetPackageValidator nu) {
        }

        protected void AddProhibitedPatternFinder(string ruleName, string prohibited, params string[] exceptions) {
            prohibited = ReplaceRoot(prohibited);
            b.Verbose.Log($"Creating prohibition {ruleName}");

            var flt = Minimatcher.CreateFilter(prohibited, o);
            var exceptFor = new List<Minimatcher>();
            foreach (string exception in exceptions) {
                string ne = ReplaceRoot(exception);
                var m = new Minimatcher(ne, o);
                exceptFor.Add(m);
            }
            bool isProhibited(string pat) {
                if (flt(pat)) {
                    foreach (var n in exceptFor) {
                        if (n.IsMatch(pat)) {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
            prohibitors.Add(new Tuple<string, Func<string, bool>>(ruleName, isProhibited));
        }

        protected virtual void AddRegexValidator(RegexLineValidator rlv) {
        }

        protected void PrepareValidators() {
            b.Verbose.Log("Validator preparation occurs");

            foreach (var f in validators) {
                if (f is DirectoryValidator dc) {
                    b.Verbose.Log($"Directory Validator Load {dc.TriggeringRule}");
                    AddDirectoryValidator(dc);
                }
                if (f is FileValidator fs) {
                    b.Verbose.Log($"FileSystem Validator Load {fs.TriggeringRule}");
                    AddFileValidator(fs);
                }
                if (f is NugetPackageValidator nu) {
                    b.Verbose.Log($"Nuget Validator Load {nu.TriggeringRule}");
                    AddNugetValidator(nu);
                }
                if (f is RegexLineValidator ra) {
                    b.Verbose.Log($"Regex Validatior Load {ra.TriggeringRule}");
                    AddRegexValidator(ra);
                }
            }
        }

        protected string ReplaceRoot(string prohibited) {
            return ps == null
                ? throw new InvalidOperationException("The root must be set in the validator prior to calling ReplaceRoot")
                : prohibited.Replace("%ROOT%", ps.Root);
        }
    }
}