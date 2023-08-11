namespace mollycoddle {
    using System.Xml.Linq;
    using Minimatch;


    /// <summary>
    /// Executes the actual checks against nuget files, taking a validation ruleset and running it against the actual files
    /// to highlight violations, such as banned packages or must have packages.
    /// </summary>
    public class NugetPackageStructureChecker : StructureCheckerBase {
        protected List<MinmatchActionCheckEntity> Actions = new();
        protected List<MinmatchActionCheckEntity> ViolatedActions = new();

        public NugetPackageStructureChecker(ProjectStructure ps, MollyOptions mo) : base(ps, mo) {
            this.ps = ps;
        }

        /// <summary>
        /// Reads the nuget package details from sdk stlye csproj, using the contents of that file ( not the filename ) as input.
        /// </summary>
        /// <param name="projectContents"></param>
        /// <returns></returns>
        public IEnumerable<NugetPackageEntry> ReadNugetPackageFromSDKProjectContents(string projectContents) {
            XDocument x;
            XElement pe;
            try {
                x = XDocument.Parse(projectContents);
                var el = x.Element("Project");
                if (el!=null) {
                    pe = el;
                } else {
                    b.Warning.Log("Invalid XML, no Project Element Found. This is not a valid csproj file.");
                    yield break;
                }
            } catch (System.Xml.XmlException ix) {
                b.Warning.Dump(ix, "Invalid XML in a project file, most likely this is not a real csproj");
                b.Warning.Log("File contents...", projectContents);
                yield break;
            }

            if (pe != null) {
                foreach (var ig in pe.Elements("ItemGroup")) {
                    foreach (var pr in ig.Elements("PackageReference")) {
                        var e1 = pr.Attribute("Include");
                        var e2 = pr.Attribute("Version");
                        if (e1 != null && e2 != null) {
                            yield return new NugetPackageEntry(e1.Value, e2.Value);
                        }
                    }
                }
            }
        }

        protected override CheckResult ActualExecuteChecks(CheckResult result) {
            foreach (string fn in ps.AllFiles) {
                int i = 0;
                while (i < Actions.Count) {
                    var chk = Actions[i];

                    if (chk.IsInViolation) {
                        continue;
                    }

                    if (chk.ExecuteCheckWasViolation(fn)) {
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
                    result.AddDefect(a.OwningRuleIdentity, a.GetViolationMessage());
                }
            }
            foreach (var l in ViolatedActions) {
                result.AddDefect(l.OwningRuleIdentity, l.GetViolationMessage());
                Actions.Add(l);
            }
            ViolatedActions.Clear();

            return result;
        }

        protected override void AddNugetValidator(NugetPackageValidator nu) {
            base.AddNugetValidator(nu);

            foreach (var l in nu.GetProhibitedPackagesLists()) {
                AddNugetBannedPackageAction(nu.TriggeringRule, l.Pattern, l.ProhibitedPackages);
            }

            foreach (var l in nu.GetMustReferencePackagesList()) {
                AddNugetMustIncludeAction(nu.TriggeringRule, l.Pattern, l.MustIncludePackages);
            }
        }

        protected virtual Action<MinmatchActionCheckEntity, string> GetBannedPackageListChecker(PackageReference[] bannedList) {
            var act = new Action<MinmatchActionCheckEntity, string>((resultant, filenameToCheck) => {
                b.Assert.True(File.Exists(filenameToCheck), "Validation that the file exists should happen before this call, dev fault");

                string fileContents = ps.GetFileContents(filenameToCheck);
                var nugetPackageReferences = ReadNugetPackageFromSDKProjectContents(fileContents);
                foreach (var nugetPackage in nugetPackageReferences) {
                    foreach (var bannedPackage in bannedList) {
                        if (string.Compare(nugetPackage.PackageIdentifier, bannedPackage.PackageName, true) == 0) {
                            resultant.IsInViolation = true;
                            resultant.ViolationMessageFormat = $"{filenameToCheck}" + " contains banned package ({0})";
                            resultant.AdditionalInfo = nugetPackage.PackageIdentifier;
                            return;
                        }
                    }
                }
            });
            return act;
        }
        protected virtual Action<MinmatchActionCheckEntity, string> GetMustIncludeListChecker(PackageReference[] mustIncludeList) {

            var act = new Action<MinmatchActionCheckEntity, string>((resultant, filenameToCheck) => {
                b.Assert.True(File.Exists(filenameToCheck), "Validation that the file exists should happen before this call, dev fault");

                string? s = ps.GetFileContents(filenameToCheck);
                var nps = ReadNugetPackageFromSDKProjectContents(s);

                foreach (var l in mustIncludeList) {
                    bool matched = false;

                    foreach (var n in nps) {
                        if (string.Compare(n.PackageIdentifier, l.PackageName, true) == 0) {
                            matched = true;
                            break;
                        }
                    }

                    if (!matched) {
                        resultant.IsInViolation = true;
                        resultant.ViolationMessageFormat = $"{filenameToCheck}" + " does not reference package ({0})";
                        resultant.AdditionalInfo = l.PackageName;
                    }
                }
            });
            return act;
        }


        protected virtual string ValidateActualPath(string pathToActual) {
            return pathToActual.Replace("%ROOT%", ps.Root);
        }

        protected virtual string ValidateMasterPath(string pathToMaster) {
            ArgumentNullException.ThrowIfNull(pathToMaster);

            pathToMaster = pathToMaster.Replace("%MASTERROOT%", mo.MasterPath);

            return !File.Exists(pathToMaster) ? throw new FileNotFoundException("master path must be present", pathToMaster) : pathToMaster;
        }

        private void AddNugetMustIncludeAction(string ruleName, string pattern, PackageReference[] mustIncludePackages) {
            pattern = ValidateActualPath(pattern);
            var fca = new MinmatchActionCheckEntity(ruleName) {
                PerformCheck = GetMustIncludeListChecker(mustIncludePackages),
                DoesMatch = new Minimatcher(pattern, o)
            };
            Actions.Add(fca);
        }

        private void AddNugetBannedPackageAction(string ruleName, string pattern, PackageReference[] prohibitedPackages) {
            pattern = ValidateActualPath(pattern);
            var fca = new MinmatchActionCheckEntity(ruleName) {
                PerformCheck = GetBannedPackageListChecker(prohibitedPackages),
                DoesMatch = new Minimatcher(pattern, o)
            };
            Actions.Add(fca);
        }
    }
}