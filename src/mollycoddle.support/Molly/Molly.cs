namespace mollycoddle {

    using System;
    using System.Collections.Generic;
    using Plisky.Diagnostics;

    public class Molly {
        protected Dictionary<string, string> ruleSupportingInfo = new Dictionary<string, string>();
        private readonly Bilge b = new Bilge("molly-main");
        private readonly MollyOptions mo;
        private DirectoryStructureChecker? dst;
        private FileStructureChecker? fst;
        private NugetPackageStructureChecker? nst;
        private bool projectStructureLoaded = false;
        public const string CFFVIOLATION = "CommonFilesFetchError";

        public Molly(MollyOptions mo) {
            this.mo = mo;
        }

        public void AddProjectStructure(ProjectStructure ps) {
            projectStructureLoaded = true;
            dst = new DirectoryStructureChecker(ps, mo);
            fst = new FileStructureChecker(ps, mo);
            nst = new NugetPackageStructureChecker(ps, mo);
        }

        public CheckResult ExecuteAllChecks() {
            if (!projectStructureLoaded) {
                throw new InvalidOperationException("Not Possible to execute checks prior to the project structure being loaded");
            }

            b.Info.Log("Executing directory structure checks");
            var cr = dst!.Check();
            b.Info.Log($"{cr.DefectCount} violations, executing file system checks");
            cr = fst!.Check(cr);
            b.Info.Log($"{cr.DefectCount} violations, executing nuget checks");
            cr = nst!.Check(cr);
            return cr;
        }

        public string GetRuleSupportingInfo(string ruleName) {
            if (ruleSupportingInfo.ContainsKey(ruleName)) {
                return ruleSupportingInfo[ruleName];
            }
            return string.Empty;
        }

        public void ImportRules(IEnumerable<MollyRule> rulesToAdd) {
            if (!projectStructureLoaded) {
                throw new InvalidOperationException("Not Possible to add rules prior to the project structure being loaded");
            }

            foreach (var x in rulesToAdd) {
                b.Info.Log($"Rule {x.Name} loaded");

                if (!ruleSupportingInfo.ContainsKey(x.Name)) {
                    string linkHelpText = "Sorry, no further information provided.";
                    if (!string.IsNullOrEmpty(x.Link)) {
                        linkHelpText = "See: " + x.Link;
                    }
                    ruleSupportingInfo.Add(x.Name, linkHelpText);
                }

                foreach (var n in x.Validators) {
                    dst!.AddRuleRequirement(n);
                    fst!.AddRuleRequirement(n);
                    nst!.AddRuleRequirement(n);
                }
            }
        }

        public Dictionary<string, string>? ApplyMollyFix() {
            bool fixApplied = false;
            string logMessage;
            var defects = new Dictionary<string, string>();
            // Only fix for file structure violations implemented
            if (fst != null && fst.violationCountTotal > 0) {
                int getResult = fst.ApplyFix();
                if (getResult == 0) {
                    logMessage = $"Common files fetched successfully.";
                    fixApplied = true;
                } else {
                    logMessage = $"Failed to fetch common files. {getResult} errors occurred.";
                    defects.Add(CFFVIOLATION, logMessage);
                }
                b.Verbose.Log(logMessage);
            }
            if ((dst != null && dst.violationsCountTotal > 0) || (nst != null && nst.violationCountTotal > 0)) {
                logMessage = "ApplyMollyFix not supported for directory or nugetpackage structure violations at this time.";
                b.Warning.Log(logMessage);
            }
            var result = (fixApplied || defects.Count > 0) ? defects : null;
            return result;
        }
    }
}