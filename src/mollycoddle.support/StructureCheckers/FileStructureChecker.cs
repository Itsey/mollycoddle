namespace mollycoddle {

    using System;
    using System.Security.Cryptography;
    using Minimatch;

    public class FileStructureChecker : StructureCheckerBase {
        protected List<MinmatchActionCheckEntity> Actions = new List<MinmatchActionCheckEntity>();
        protected List<MinmatchActionCheckEntity> ViolatedActions = new List<MinmatchActionCheckEntity>();

        public FileStructureChecker(ProjectStructure ps, MollyOptions mo) : base(ps, mo) {
            
        }

        public virtual void AssignCompareWithMasterAction(string patternToMatch, string pathToMaster, string ruleName) {
            patternToMatch = ValidateActualPath(patternToMatch);
            string pm = ValidateMasterPath(pathToMaster);

            var fca = new MinmatchActionCheckEntity(ruleName);
            fca.PerformCheck = GetContentsCheckerAction(pm);
            fca.DoesMatch = new Minimatcher(patternToMatch, o);

            Actions.Add(fca);
        }

        public void AssignFileMustNotContainAction(string violationRuleName, string patternForFile, string textToFind) {
            var fca = new MinmatchActionCheckEntity(violationRuleName);
            fca.PerformCheck = GetFileContentsMustChecker(textToFind, false);
            fca.DoesMatch = new Minimatcher(patternForFile, o);
            Actions.Add(fca);
        }

        /// <summary>
        /// If the primary pattern exists it must match one of the patterns in the secondary patterns. Not supplying any secondary patterns is an error.
        /// </summary>
        /// <param name="ruleName">The name of the owning rule</param>
        /// <param name="patternForFiles">A primary / secondary combo describing the rules</param>
        public void AssignIfItExistsItMustBeHereAction(string ruleName, MatchWithSecondaryMatches patternForFiles) {
            var fca = new MinmatchActionCheckEntity(ruleName);
            fca.PerformCheck = GetMustMatchOneOfTheseChecker(patternForFiles.SecondaryList);
            fca.DoesMatch = new Minimatcher(patternForFiles.PrimaryPattern, o);
            Actions.Add(fca);
        }



        public void AssignMustExistAction(string ruleName, string patternForFile) {
            var fca = new MinmatchActionCheckEntity(ruleName);
            // Must Exist defaults to having not passed.  It passes if the file is then found.
            patternForFile = ValidateActualPath(patternForFile);
            fca.Passed = false;
            fca.AdditionalInfo = $"{patternForFile} must exist.";
            fca.PerformCheck = GetFileExistChecker();
            fca.DoesMatch = new Minimatcher(patternForFile, o);

            Actions.Add(fca);
        }
        public void AssignMustNotExistAction(string ruleName, MatchWithSecondaryMatches patternForFile) {
            var fca = new MinmatchActionCheckEntity(ruleName);
            // Must Exist defaults to having not passed.  It passes if the file is then found.
            string pff = ValidateActualPath(patternForFile.PrimaryPattern);
            fca.Passed = true;
            fca.AdditionalInfo = $"{patternForFile} must not exist.";
            fca.PerformCheck = GetFileExistChecker(false, patternForFile.SecondaryList);
            fca.DoesMatch = new Minimatcher(pff, o);

            Actions.Add(fca);
        }

        protected override CheckResult ActualExecuteChecks(CheckResult result) {

            if (Actions.Count == 0) {
                b.Warning.Log("There are no loaded actions for the file structure checker, no file based checks occur.");
                return result;
            }

            b.Info.Log($"File System  Check {ps.AllFiles.Count} files to check");
            foreach (string fn in ps.AllFiles) {

                bool bypassActive = false;
                foreach (var bp in bypassMatch) {
                    if (bp(fn)) {
                        bypassActive = true;
                        break;
                    }
                }

                if (bypassActive) {
                    b.Verbose.Log($"bypass filter activated for {fn}");
                    continue;
                }

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
                    b.Verbose.Log($"{a.OwningRuleIdentity} failed.  After all checks it was not marked as passed.  Additional Info: {a.AdditionalInfo}");
                    result.AddDefect(a.OwningRuleIdentity, a.GetViolationMessage());
                }
            }
            foreach (var l in ViolatedActions) {
                b.Verbose.Log($"Violation: {l.OwningRuleIdentity} failed {l.AdditionalInfo}");
                result.AddDefect(l.OwningRuleIdentity, l.GetViolationMessage());
                Actions.Add(l);
            }
            ViolatedActions.Clear();

            return result;
        }

        protected override void AddFileValidator(FileValidator fs) {
            base.AddFileValidator(fs);

            foreach(string l in fs.FullBypasses()) {
                AddFullbypassActions(l);
            }
            foreach (var l in fs.FilesThatMustMatchTheirMaster()) {
                AssignCompareWithMasterAction(ValidateActualPath(l.PatternForSourceFile), l.FullPathForMasterFile, fs.TriggeringRule);
            }
            foreach (string ln in fs.FilesThatMustExist()) {
                AssignMustExistAction(fs.TriggeringRule, ln);
            }
            foreach (var lnn in fs.FilesThatMustNotExist()) {
                AssignMustNotExistAction(fs.TriggeringRule, lnn);
            }
            foreach (var lnnn in fs.FilesInSpecificPlaces()) {
                AssignIfItExistsItMustBeHereAction(fs.TriggeringRule, lnnn);
            }
        }

        private void AddFullbypassActions(string l) {
            AddMasterByPass(l.Replace("%ROOT%", ps.Root).ToLowerInvariant());
        }

        protected virtual Action<MinmatchActionCheckEntity, string> GetContentsCheckerAction(string masterContentsPath) {

            var masterLengthAndHash = ps.GetFileHashAndLength(masterContentsPath);

            var result = new Action<MinmatchActionCheckEntity, string>((resultant, filenameToCheck) => {

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
            return result;
        }

        protected virtual string? GetFileContents(string path) {
            return File.ReadAllText(path);
        }

        private Action<MinmatchActionCheckEntity, string> GetMustMatchOneOfTheseChecker(string[] secondaryList) {
            if ((secondaryList == null) || (secondaryList.Length == 0)) {
                throw new InvalidOperationException("The rule is invalid, you must have a match for a forced match rule.");
            }

            var possiblePatterns = new List<Minimatcher>();
            foreach (string l in secondaryList) {
                possiblePatterns.Add(new Minimatcher(l, o));
            }

            var result = new Action<MinmatchActionCheckEntity, string>((resultant, filenameToCheck) => {
                bool hasMatched = false;
                foreach (var mtch in possiblePatterns) {
                    if (mtch.IsMatch(filenameToCheck)) {
                        hasMatched = true;
                        break;
                    }
                }

                b.Verbose.Log($"MustMatchOneOfThese for {filenameToCheck} result {hasMatched}");

                if (!hasMatched) {
                    resultant.Passed = false;
                    resultant.IsInViolation = true;
                    resultant.AdditionalInfo = filenameToCheck;
                }
            });
            return result;
        }

        protected virtual Action<MinmatchActionCheckEntity, string> GetFileExistChecker(bool shouldExist = true, string[]? exceptionList = null) {

            List<Minimatcher>? possiblePatterns=null;

            if (exceptionList != null) {
                possiblePatterns = new List<Minimatcher>();

                foreach (string l in exceptionList) {
                    possiblePatterns.Add(new Minimatcher(l, o));
                }

            }

            var result = new Action<MinmatchActionCheckEntity, string>((resultant, filenameToCheck) => {

                if (possiblePatterns != null) {
                    foreach (var l in possiblePatterns) {
                        if (l.IsMatch(filenameToCheck)) {
                            resultant.Passed = true;
                            return;
                        }
                    }
                }
            
                if (ps.DoesFileExist(filenameToCheck) == shouldExist) {
                    resultant.Passed = true;
                } else {
                    resultant.IsInViolation = true;
                    resultant.AdditionalInfo = filenameToCheck;
                }
            });

            
            return result;
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

        private Action<MinmatchActionCheckEntity, string> GetFileContentsMustChecker(string text, bool mustOrMustNot) {
            return new Action<MinmatchActionCheckEntity, string>((resultant, filenameToCheck) => {
                string? f = GetFileContents(filenameToCheck);
                if (f != null) {
                    resultant.IsInViolation = f.Contains(text, StringComparison.OrdinalIgnoreCase) != mustOrMustNot;
                    resultant.AdditionalInfo = filenameToCheck;
                }
            });
        }
    }
}