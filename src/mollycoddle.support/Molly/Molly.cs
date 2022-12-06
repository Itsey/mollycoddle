namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Plisky.Diagnostics;

    public class Molly {
        private readonly Bilge b = new Bilge("molly-main");
        private readonly MollyOptions mo;
        private DirectoryStructureChecker? dst;
        private FileStructureChecker? fst;
        private NugetPackageStructureChecker? nst;
        private bool projectStructureLoaded = false;

        public Molly(MollyOptions mo) {
            this.mo = mo;
        }

        public void AddProjectStructure(ProjectStructure ps) {
            projectStructureLoaded = true;
            dst = new DirectoryStructureChecker(ps, mo);
            fst = new FileStructureChecker(ps, mo);
            nst = new NugetPackageStructureChecker(ps, mo);
        }

        protected Dictionary<string, string> ruleSupportingInfo = new Dictionary<string, string>();
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
                        linkHelpText = "See: "+x.Link;
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
    }
}
