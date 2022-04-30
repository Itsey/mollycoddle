namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Minimatch;
    using Plisky.Diagnostics;


    /// <summary>
    /// The job here is to actually check the directory structure for failures.
    /// </summary>
    public class DirectoryStructureChecker  {
        protected Bilge b = new Bilge("molly-directorycheck");
        private Options o = new Options() { AllowWindowsPaths = true, IgnoreCase = true };

        protected List<ValidatorBase> validators = new List<ValidatorBase>();
        protected List<Tuple<string,Func<string,bool>>> prohibitors = new List<Tuple<string,Func<string,bool>>>();
        protected string ReplaceRoot(string prohibited) {
            if (ps == null) {
                throw new InvalidOperationException("The root must be set in the validator prior to calling ReplaceRoot");
            }
            return prohibited.Replace("%ROOT%", ps.Root);
        }

        public void AddRuleRequirement(ValidatorBase n) {
            validators.Add(n);
        }

        private ProjectStructure ps;

        public DirectoryStructureChecker(ProjectStructure ps) {
            this.ps = ps;
        }

        public void AddProhibitedPatternFinder(string ruleName, string prohibited, params string[] exceptions) {
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
            prohibitors.Add(new Tuple<string, Func<string, bool>>(ruleName,isProhibited));

        }

        public CheckResult CheckDirectories() {
            b.Info.Flow();

            var result = new CheckResult();

            Dictionary<string, CheckEntity> directoriesThatMustExist = new Dictionary<string, CheckEntity>();

            foreach (var f in validators) {
                if (f is DirectoryValidationChecks dc) {

                    foreach (var l in dc.MustExistExactly()) {
                        var nl = l.Replace("%ROOT%", ps.Root).ToLowerInvariant();
                        directoriesThatMustExist.Add(nl, new CheckEntity(dc.TriggeringRule));
                    }
                    foreach(var l in dc.GetProhibitedPaths()) {
                        AddProhibitedPatternFinder(dc.TriggeringRule,l.ProhibitedPattern,l.ExceptionsList);
                    }
                }
            }


            foreach (var l in ps.AllFolders) {
                var j = l.ToLowerInvariant();
                b.Verbose.Log($"checking {j}");

                if (directoriesThatMustExist.ContainsKey(j)) {
                    directoriesThatMustExist[j].Passed = true;
                }

                foreach (var isPathProhibited in prohibitors) {
                    if (isPathProhibited.Item2(j)) {
                        result.AddDefect(isPathProhibited.Item1, j);
                    };
                }

            }

            // Validation, did everything pass
            foreach (var k in directoriesThatMustExist.Keys) {
                if (!directoriesThatMustExist[k].Passed) {
                    result.AddDefect(directoriesThatMustExist[k].OwningRuleIdentity,k);
                }
            }



            return result;
        }


    }
}
