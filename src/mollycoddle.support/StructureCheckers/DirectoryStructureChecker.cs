namespace mollycoddle {

    using System.Collections.Generic;

    /// <summary>
    /// The job here is to actually check the directory structure for failures.
    /// </summary>
    public class DirectoryStructureChecker : StructureCheckerBase {
        private Dictionary<string, CheckEntityBase> directoriesThatMustExist = new Dictionary<string, CheckEntityBase>();

        public DirectoryStructureChecker(ProjectStructure ps, MollyOptions mo) : base(ps, mo) {
        }

        protected override CheckResult ActualExecuteChecks(CheckResult result) {
            b.Info.Flow();

            foreach (var nextFolder in ps.AllFolders) {
                var folderName = nextFolder.ToLowerInvariant();
                b.Verbose.Log($"checking {folderName}");

                if (IsByPassActive(folderName)) {
                    b.Verbose.Log($"bypass filter activated for {folderName}");
                    continue;
                }
                
                if (directoriesThatMustExist.ContainsKey(folderName)) {
                    directoriesThatMustExist[folderName].Passed = true;
                }

                foreach (var isPathProhibited in prohibitors) {
                    if (isPathProhibited.Item2(folderName)) {
                        result.AddDefect(new Violation(isPathProhibited.Item1) {
                            Additional = $"({folderName}) is a prohibited path."
                        });
                        
                    };
                }
            }

            // Validation, did everything pass
            foreach (var k in directoriesThatMustExist.Keys) {
                if (!directoriesThatMustExist[k].Passed) {
                    result.AddDefect(new Violation(directoriesThatMustExist[k].OwningRuleIdentity) {
                        Additional = $"({k}) must exist and it does not."
                    });
                }
            }

            return result;
        }

        private bool IsByPassActive(string j) {
            bool bypassActive = false;

            foreach (var bp in bypassMatch) {
                if (bp(j)) {
                    bypassActive = true;
                    break;
                }
            }
            return bypassActive;
        }

        protected override void AddDirectoryValidator(DirectoryValidator dc) {
            base.AddDirectoryValidator(dc);

            foreach (string masterBypassPattern in dc.FullBypasses()) {
                AddMasterByPass(masterBypassPattern.Replace("%ROOT%", ps.Root).ToLowerInvariant());
            }

            foreach (var l in dc.MustExistExactly()) {
                string nl = l.Replace("%ROOT%", ps.Root).ToLowerInvariant();
                directoriesThatMustExist.Add(nl, new CheckEntityBase(dc.TriggeringRule) {
                    Passed = false
                });
                b.Verbose.Log($"{dc.TriggeringRule} Loading mustexist path : {nl}");
            }

            foreach (var l in dc.GetProhibitedPaths()) {
                b.Verbose.Log($"{dc.TriggeringRule} Loading prohibited path : {l.PrimaryPattern} with {l.SecondaryList.Length} exceptions");
                AddProhibitedPatternFinder(dc.TriggeringRule, l.PrimaryPattern, l.SecondaryList);
            }
        }
    }
}