namespace mollycoddle {

    using System;
    using System.Security.Cryptography;
    using Minimatch;

    public class FileStructureChecker : StructureCheckerBase {
        protected List<MinMatchActionChecker> Actions = new List<MinMatchActionChecker>();
        protected List<MinMatchActionChecker> ViolatedActions = new List<MinMatchActionChecker>();

        public FileStructureChecker(ProjectStructure ps, MollyOptions mo) : base(ps, mo) {
            this.ps = ps;
        }

        public virtual void AssignCompareWithMasterAction(string patternToMatch, string pathToMaster, string ruleName) {
            patternToMatch = ValidateActualPath(patternToMatch);
            string pm = ValidateMasterPath(pathToMaster);

            var fca = new MinMatchActionChecker(ruleName);
            fca.PerformCheck = GetContentsCheckerAction(pm);
            fca.DoesMatch = new Minimatcher(patternToMatch, o);

            Actions.Add(fca);
        }

        public void AssignFileMustNotContainAction(string violationRuleName, string patternForFile, string textToFind) {
            var fca = new MinMatchActionChecker(violationRuleName);
            fca.PerformCheck = GetFileContentsMustChecker(textToFind, false);
            fca.DoesMatch = new Minimatcher(patternForFile, o);
            Actions.Add(fca);
        }

        public void AssignMustExistAction(string ruleName, string patternForFile) {
            var fca = new MinMatchActionChecker(ruleName);
            // Must Exist defaults to having not passed.  It passes if the file is then found.
            patternForFile = ValidateActualPath(patternForFile);
            fca.Passed = false;
            fca.AdditionalInfo = $"{patternForFile} must exist.";
            fca.PerformCheck = GetFileExistChecker();
            fca.DoesMatch = new Minimatcher(patternForFile, o);

            Actions.Add(fca);
        }
        public void AssignMustNotExistAction(string ruleName, ProhibitedPathSet patternForFile) {
            var fca = new MinMatchActionChecker(ruleName);
            // Must Exist defaults to having not passed.  It passes if the file is then found.
            var pff = ValidateActualPath(patternForFile.ProhibitedPattern);
            fca.Passed = true;
            fca.AdditionalInfo = $"{patternForFile} must not exist.";
            fca.PerformCheck = GetFileExistChecker(false);
            fca.DoesMatch = new Minimatcher(pff, o);

            Actions.Add(fca);
        }

        protected override CheckResult ActualExecuteChecks(CheckResult result) {
            
            if (Actions.Count == 0) {
                b.Warning.Log("There are no loaded actions for the file structure checker, no file based checks occur.");
                return result;
            }

            b.Info.Log($"File System  Check {ps.AllFiles.Count} files to check");
            foreach (var fn in ps.AllFiles) {
                int i = 0;
                while (i < Actions.Count) {
                    var chk = Actions[i];

                    if (chk.IsInViolation) {
                        continue;
                    }

                    if (chk.ExecuteCheckWasViolation(fn)) {
                        b.Verbose.Log($"Violation {fn}, {chk.OwningRuleIdentity} {chk.AdditionalInfo}");
                        ViolatedActions.Add(chk);
                        Actions.Remove(chk);
                    } else {
                        i++;
                    }
                }
            }

            // Loop to catch those violations which require to actively pass, e.g. Must exist or Must not exist, they
            // need to check each file to determine if they have passed or not.
            foreach (var a in Actions) {
                if (!a.Passed) {
                    b.Verbose.Log($"{a.OwningRuleIdentity} failed on not passed after all checks {a.AdditionalInfo}");
                    result.AddDefect(a.OwningRuleIdentity, a.AdditionalInfo);
                }
            }
            foreach (var l in ViolatedActions) {
                b.Verbose.Log($"Violation: {l.OwningRuleIdentity} failed {l.AdditionalInfo}");
                result.AddDefect(l.OwningRuleIdentity, l.AdditionalInfo);
                Actions.Add(l);
            }
            ViolatedActions.Clear();

            return result;
        }

        protected override void AddFileValidator(FileValidationChecks fs) {
            base.AddFileValidator(fs);

            foreach (var l in fs.FilesThatMustMatchTheirMaster()) {
                AssignCompareWithMasterAction(ValidateActualPath(l.PatternForSourceFile), l.FullPathForMasterFile, fs.TriggeringRule);
            }
            foreach (var ln in fs.FilesThatMustExist()) {
                AssignMustExistAction(fs.TriggeringRule, ln);
            }
            foreach (var lnn in fs.FilesThatMustNotExist()) {
                AssignMustNotExistAction(fs.TriggeringRule, lnn);
            }
        }

        protected virtual Action<MinMatchActionChecker, string> GetContentsCheckerAction(string masterContentsPath) {
        
            var masterLengthAndHash= ps.GetFileHashAndLength(masterContentsPath);

            var act = new Action<MinMatchActionChecker, string>((resultant, filenameToCheck) => {

                var targetLengthAndHasn = ps.GetFileHashAndLength(filenameToCheck);

                if (targetLengthAndHasn.Item1 == masterLengthAndHash.Item1) {
                    if (!masterLengthAndHash.Item2.SequenceEqual(targetLengthAndHasn.Item2)) {
                        // Failed
                        resultant.IsInViolation = true;
                        resultant.AdditionalInfo = filenameToCheck;
                    }
                } else {
                    resultant.IsInViolation = true;
                    resultant.AdditionalInfo = $"{filenameToCheck} does not match master.";
                }
            });
            return act;
        }

        protected virtual string GetFileContents(string path) {
            return File.ReadAllText(path);
        }

        protected virtual Action<MinMatchActionChecker, string> GetFileExistChecker(bool shouldExist=true) {

            var act = new Action<MinMatchActionChecker, string>((resultant, filenameToCheck) => {
                
                if (ps.DoesFileExist(filenameToCheck) == shouldExist) {
                    resultant.Passed = true;
                } else {
                    resultant.IsInViolation = true;
                    resultant.AdditionalInfo = filenameToCheck;
                }
            });
            return act;
        }

        protected virtual string ValidateActualPath(string pathToActual) {
            return pathToActual.Replace("%ROOT%", ps.Root);
        }

        protected virtual string ValidateMasterPath(string pathToMaster) {
            b.Info.Flow(pathToMaster);
            ArgumentNullException.ThrowIfNull(pathToMaster);

            pathToMaster = pathToMaster.Replace("%MASTERROOT%", mo.MasterPath);
            b.Verbose.Log($"resolved master path {pathToMaster} using {mo.MasterPath}");
            if (!ps.DoesFileExist(pathToMaster)) {
                b.Error.Log($"Error, master path file is not found {pathToMaster}");
                throw new FileNotFoundException($"Master path ({pathToMaster}) must be present.", pathToMaster);
            }

            return pathToMaster;
        }

        private Action<MinMatchActionChecker, string> GetFileContentsMustChecker(string text, bool mustOrMustNot) {
            var act = new Action<MinMatchActionChecker, string>((resultant, filenameToCheck) => {
                var f = GetFileContents(filenameToCheck);
                resultant.IsInViolation = f.Contains(text, StringComparison.OrdinalIgnoreCase) != mustOrMustNot;
                resultant.AdditionalInfo = filenameToCheck;
            });
            return act;
        }
    }
}