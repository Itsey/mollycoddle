namespace mollycoddle {

    public class RegexStructureChecker : StructureCheckerBase {
        protected List<RegexLineCheckEntity> Actions = new();
        protected List<RegexLineCheckEntity> ViolatedActions = new();

        public RegexStructureChecker(ProjectStructure ps, MollyOptions mopts) : base(ps, mopts) {
        }

        protected override CheckResult ActualExecuteChecks(CheckResult result) {
            b.Verbose.Flow();

            if (Actions.Count == 0) {
                b.Warning.Log("There are no loaded actions for the Regex Checker, no violations can occur.");
                return result;
            }

            b.Info.Log($"Regex Check {ps.AllFiles.Count} files to check");
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
                    b.Verbose.Log($"Executing Check Action {chk.OwningRuleIdentity}");
                    if (chk.IsInViolation) {
                        continue;
                    }

                    if (chk.ExecuteCheckWasViolation(fn)) {
                        b.Verbose.Log($"Violation {fn}, {chk.OwningRuleIdentity} {chk.AdditionalInfo}");
                        ViolatedActions.Add(chk);
                        _ = Actions.Remove(chk);
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

        protected override void AddRegexValidator(RegexLineValidator rlv) {
            b.Verbose.Log("NugetValidator added ");

            switch (rlv.MatchType) {
                case RegexBehaviour.MustMatchOnce:
                    AssignMustMatchOnceAction(rlv);
                    break;

                case RegexBehaviour.MustNotMatch:
                    AssignMustNotMatchAction(rlv);
                    break;

                case RegexBehaviour.ValueMustEqual: break;
                case RegexBehaviour.ValueMustNotEqual: break;
            }
        }

        private void AssignMustMatchOnceAction(RegexLineValidator rlv) {
            b.Verbose.Flow();
            throw new NotImplementedException();
        }

        private void AssignMustNotMatchAction(RegexLineValidator rlv) {
            b.Verbose.Flow();
            b.Assert.NotNull(rlv.FileMinmatch);

            var rca = new RegexLineCheckEntity(rlv.TriggeringRule) {
                PerformCheck = new Action<RegexLineCheckEntity, string>((resultant, fileToCheck) => {
                    if (rlv.FileMinmatch.IsMatch(fileToCheck)) {
                        string? file = ps.GetFileContents(fileToCheck);
                        if (string.IsNullOrEmpty(file)) {
                            b.Verbose.Log("No file contents found, not checking anything");
                        } else {
                            b.Verbose.Log($"Parsing File Contents {fileToCheck}");

                            foreach (string l in file.Split(Environment.NewLine)) {
                                if (rlv.RegexMatch.IsMatch(l)) {
                                    b.Info.Log("Regex Match occured", l);
                                    resultant.IsInViolation = true;
                                    resultant.AdditionalInfo = string.Format(resultant.GetViolationMessage(), l, fileToCheck);
                                    break;
                                }
                            }
                        }
                    }
                })
            };

            Actions.Add(rca);
        }
    }
}