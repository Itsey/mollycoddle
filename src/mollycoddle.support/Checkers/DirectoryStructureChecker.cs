namespace mollycoddle {

    using System.Collections.Generic;

    /// <summary>
    /// The job here is to actually check the directory structure for failures.
    /// </summary>
    public class DirectoryStructureChecker : StructureCheckerBase {
        private Dictionary<string, CheckEntity> directoriesThatMustExist = new Dictionary<string, CheckEntity>();

        public DirectoryStructureChecker(ProjectStructure ps, MollyOptions mo) : base(ps, mo) {
        }

        protected override CheckResult ActualExecuteChecks(CheckResult result) {
            b.Info.Flow();

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
                    result.AddDefect(directoriesThatMustExist[k].OwningRuleIdentity, k);
                }
            }

            return result;
        }

        protected override void AddDirectoryValidator(DirectoryValidationChecks dc) {
            base.AddDirectoryValidator(dc);

            foreach (var l in dc.MustExistExactly()) {
                string nl = l.Replace("%ROOT%", ps.Root).ToLowerInvariant();
                directoriesThatMustExist.Add(nl, new CheckEntity(dc.TriggeringRule) {
                    Passed = false
                });
                b.Verbose.Log($"{dc.TriggeringRule} Loading mustexist path : {nl}");
            }

            foreach (var l in dc.GetProhibitedPaths()) {
                b.Verbose.Log($"{dc.TriggeringRule} Loading prohibited path : {l.ProhibitedPattern} with {l.ExceptionsList.Length} exceptions");
                AddProhibitedPatternFinder(dc.TriggeringRule, l.ProhibitedPattern, l.ExceptionsList);
            }
        }
    }
}