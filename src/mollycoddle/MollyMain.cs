namespace mollycoddle {
    using System;
    using System.Threading.Tasks;
    using Plisky.Diagnostics;
    using Plisky.Plumbing;

    internal class MollyMain {
        protected Bilge b;
        protected MollyOptions mo;
        protected string? basePathToSave = null;

        public Action<string, OutputType> WriteOutput { get; set; } = (a, b) => { };

        public MollyMain(MollyOptions options, Bilge bilgeInstance) {
            mo = options;
            b = bilgeInstance;
        }

        internal async Task<CheckResult> DoMollly() {
            var result = new CheckResult();
            b.Info.Flow();

            ValidateMollyOptions();

            b.Verbose.Log($"Targeting Directory ]{mo.DirectoryToTarget}");


            await HandleMcRuleSources(mo);
            Hub.Current.Launch(new CheckpointMessage { Name = "rules." });

            var ps = new ProjectStructure {
                Root = mo.DirectoryToTarget
            };
            ps.PopulateProjectStructure();

            var mrf = new MollyRuleFactory();
            var molly = new Molly(mo);
            molly.AddProjectStructure(ps);
            try {
                molly.ImportRules(mrf.LoadRulesFromFile(mo.RulesFile));
            } catch (InvalidOperationException iox) {
                WriteOutput($"Error - Unable To Read RulesFiles", OutputType.Error);
                Exception? eox = iox;
                while (eox != null) {
                    WriteOutput($"Error: {eox.Message}", OutputType.Error);
                    eox = eox.InnerException;
                }
                throw;
            }

            result = molly.ExecuteAllChecks();
            Hub.Current.Launch(new CheckpointMessage { Name = "checks." });



            string lastWrittenRule = string.Empty;
            foreach (var l in result.ViolationsFound.OrderBy(p => p.RuleName)) {
                if (mo.AddHelpText) {
                    if (l.RuleName != lastWrittenRule) {
                        WriteOutput($"❓ {l.RuleName} Further help:  {molly.GetRuleSupportingInfo(l.RuleName)}", OutputType.Info);
                        lastWrittenRule = l.RuleName;
                    }
                    WriteOutput($"{l.Additional}", OutputType.Violation);
                } else {
                    WriteOutput($"{l.RuleName} ({l.Additional})", OutputType.Violation);
                }
            }

            return result;
        }

        private void ValidateMollyOptions() {
            if (!ValidateDirectory(mo.DirectoryToTarget)) {
                WriteOutput($"InvalidCommand - Directory Was Not Correct (Does this directory exist? [{mo.DirectoryToTarget}])", OutputType.Error);
                throw new DirectoryNotFoundException($"Directory not found [{mo.DirectoryToTarget}]");
            }

            if (!ValidateRulesFile(mo.RulesFile)) {
                WriteOutput($"InvalidCommand - RulesFile was not correct (Does this rules file exist? [{mo.RulesFile}])", OutputType.Error);
                throw new FileNotFoundException($"Rules file not found [{mo.RulesFile}]");
            }
        }

        private static bool ValidateRulesFile(string rulesFile) {
            return !string.IsNullOrWhiteSpace(rulesFile);
        }

        private static bool ValidateDirectory(string pathToCheck) {
            return !string.IsNullOrWhiteSpace(pathToCheck) && Directory.Exists(pathToCheck);
        }




        private async Task HandleMcRuleSources(MollyOptions mo) {
            var fileManager = new NexusSupport();

            basePathToSave = mo.TempPath;

            Action<byte[], string, string> saver = (fileContents, fileName, identifier) => {
                if (basePathToSave == null) {
                    throw new InvalidOperationException("The base path to save has not been set.");
                }
                var parts = fileManager.GetVersionAndFilenameFromNexusUrl(identifier, fileName);
                string localDir = Path.Combine(basePathToSave, parts.Item1);
                string localFile = Path.Combine(localDir, parts.Item2);

                if (!Directory.Exists(localDir)) {
                    Directory.CreateDirectory(localDir);
                }

                File.WriteAllBytes(localFile, fileContents);
            };


            if (mo.RulesFile.StartsWith(NexusSupport.NEXUS_PREFIX)) {
                var ns = fileManager.GetNexusSettings(mo.RulesFile);
                if (ns != null) {
                    string nexusMollyMarker = "/molly";
                    string urlToUse = ns.Url.Substring(ns.Url.IndexOf(nexusMollyMarker));
                    await fileManager.CacheNexusFiles(ns, "/molly", saver);
                    var fnn = fileManager.GetVersionAndFilenameFromNexusUrl("/molly", urlToUse);
                    mo.RulesFile = Path.Combine(basePathToSave, fnn.Item1, fnn.Item2);
                }
            }

            if ((!string.IsNullOrEmpty(mo.PrimaryFilePath)) && (mo.PrimaryFilePath.StartsWith(NexusSupport.NEXUS_PREFIX))) {
                var ns = fileManager.GetNexusSettings(mo.PrimaryFilePath);
                if (ns != null) {
                    string nexusMollyMarker = "/primaryfiles";
                    string urlToUse = ns.Url.Substring(ns.Url.IndexOf(nexusMollyMarker));
                    await fileManager.CacheNexusFiles(ns, nexusMollyMarker, saver);
                    var fnn = fileManager.GetVersionAndFilenameFromNexusUrl(nexusMollyMarker, urlToUse);
                    string rulesFile = Path.GetFileName(mo.PrimaryFilePath);
                    mo.PrimaryFilePath = Path.Combine(basePathToSave, fnn.Item1);
                }
            }
        }
    }
}
