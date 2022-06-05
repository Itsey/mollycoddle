namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Plisky.Diagnostics;

    public class Molly {
        private Bilge b = new Bilge("molly-main");
        private MollyOptions mo;
        private DirectoryStructureChecker? dst;
        private FileStructureChecker? fst;
        private NugetPackageChecker? nst;
        private bool ProjectStructureLoaded = false;

        public Molly(MollyOptions mo) {
            this.mo = mo;
        }

        public void AddProjectStructure(ProjectStructure ps) {
            ProjectStructureLoaded = true;
            dst = new DirectoryStructureChecker(ps, mo);
            fst = new FileStructureChecker(ps, mo);
            nst = new NugetPackageChecker(ps, mo);
        }

        public void ImportRules(IEnumerable<MollyRule> rulesToAdd) {
            if (!ProjectStructureLoaded) {
                throw new InvalidOperationException("Not Possible to add rules prior to the project structure being loaded");
            }

            foreach (var x in rulesToAdd) {
                b.Info.Log($"Rule {x.Name} loaded");

                foreach (var n in x.Validators) {
                    dst.AddRuleRequirement(n);
                    fst.AddRuleRequirement(n);
                    nst.AddRuleRequirement(n);
                }
            }
        }

        public CheckResult ExecuteAllChecks() {
            if (!ProjectStructureLoaded) {
                throw new InvalidOperationException("Not Possible to execute checks prior to the project structure being loaded");
            }

            b.Info.Log("Executing directory structure checks");
            var cr = dst.Check();
            b.Info.Log($"{cr.DefectCount} violiations, executing file system checks");
            cr = fst.Check(cr);
            b.Info.Log($"{cr.DefectCount} violiations, executing nuget checks");
            cr = nst.Check(cr);
            return cr;
        }
    }
}
