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
                if (el != null) {
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
                if (string.IsNullOrEmpty(filenameToCheck)) {
                    throw new InvalidOperationException("Should not be performing checks on invalid filenames");
                }

                string? fileContents = ps.GetFileContents(filenameToCheck);
                IEnumerable<NugetPackageEntry> nugetPackageReferences;
                if (fileContents == null) {
                    nugetPackageReferences = new NugetPackageEntry[0];
                } else {
                    nugetPackageReferences = ReadNugetPackageFromSDKProjectContents(fileContents);
                }

                int packageCountCheck = 0;
                foreach (var nugetPackage in nugetPackageReferences) {
                    packageCountCheck++;
                    foreach (var bannedPackage in bannedList) {
                        if (string.Compare(nugetPackage.PackageIdentifier, bannedPackage.PackageName, true) == 0) {
                            b.Info.Log($"Package Name Match {nugetPackage.PackageIdentifier} looking into versioning specifics. MatchType: {bannedPackage.VersionMatchType}");

                            switch (bannedPackage.VersionMatchType) {
                                case PackageVersionMatchType.AllVersions:
                                    resultant.IsInViolation = true;
                                    resultant.ViolationMessageFormat = $"{filenameToCheck}" + " contains banned package ({0})";
                                    resultant.AdditionalInfo = nugetPackage.PackageIdentifier;
                                    return;

                                case PackageVersionMatchType.RangeProhibited:
                                    if ((nugetPackage.Version >= bannedPackage.LowVersionNumber) && (nugetPackage.Version <= bannedPackage.HighVersionNumber)) {
                                        b.Verbose.Log($"PackageVersion Failure, package {nugetPackage.PackageIdentifier} version {nugetPackage.Version} is within banned range {bannedPackage.LowVersionNumber}-{bannedPackage.HighVersionNumber}");
                                        resultant.IsInViolation = true;
                                        resultant.ViolationMessageFormat = $"{filenameToCheck} contains banned package version, less than minimum. ({{0}}) within banned range {bannedPackage.LowVersionNumber}-{bannedPackage.HighVersionNumber}";
                                        resultant.AdditionalInfo = $"{nugetPackage.PackageIdentifier}({nugetPackage.Version.ToString()})";
                                        return;
                                    }
                                    break;

                                case PackageVersionMatchType.Exact:
                                    if (nugetPackage.Version.Equals(bannedPackage.LowVersionNumber)) {
                                        b.Verbose.Log($"PackageVersion Failure, package {nugetPackage.PackageIdentifier} matched banned version number {bannedPackage.LowVersionNumber}");
                                        resultant.IsInViolation = true;
                                        resultant.ViolationMessageFormat = $"{filenameToCheck}" + " contains banned package version, less than minimum. ({0})";
                                        resultant.AdditionalInfo = $"{nugetPackage.PackageIdentifier}({nugetPackage.Version.ToString()})";
                                        return;
                                    }
                                    break;

                                case PackageVersionMatchType.NotLessThan:
                                    if (nugetPackage.Version < bannedPackage.LowVersionNumber) {
                                        b.Verbose.Log($"PackageVersion Failure, package {nugetPackage.PackageIdentifier} is less than minimum verison {bannedPackage.LowVersionNumber}");
                                        resultant.IsInViolation = true;
                                        resultant.ViolationMessageFormat = $"{filenameToCheck}" + " contains banned package version, less than minimum. ({0})";
                                        resultant.AdditionalInfo = $"{nugetPackage.PackageIdentifier}({nugetPackage.Version.ToString()})";
                                        return;
                                    }
                                    break;

                                case PackageVersionMatchType.NotMoreThan:
                                    if (nugetPackage.Version > bannedPackage.HighVersionNumber) {
                                        b.Verbose.Log($"PackageVersion Failure, package {nugetPackage.PackageIdentifier} is greater than maximum version {bannedPackage.HighVersionNumber}");
                                        resultant.IsInViolation = true;
                                        resultant.ViolationMessageFormat = $"{filenameToCheck}" + " contains banned package version, greater than maximum. ({0})";
                                        resultant.AdditionalInfo = $"{nugetPackage.PackageIdentifier}({nugetPackage.Version.ToString()})";
                                        return;
                                    }
                                    break;

                                default:
                                    throw new InvalidOperationException($"Unknown PackageVersionMatchType {bannedPackage.VersionMatchType}");
                            }
                        }
                    }
                }
                if (packageCountCheck == 0) {
                    b.Warning.Log($"Project File >> {filenameToCheck} - contained no nuget packages to check.");
                } else {
                    b.Verbose.Log($"Checked {packageCountCheck} packages for banned list.");
                }
            });
            return act;
        }

        protected virtual Action<MinmatchActionCheckEntity, string> GetMustIncludeListChecker(PackageReference[] mustIncludeList) {
            var act = new Action<MinmatchActionCheckEntity, string>((resultant, filenameToCheck) => {
                string? s = ps.GetFileContents(filenameToCheck);
                IEnumerable<NugetPackageEntry> packagesToCheck;
                if (s == null) {
                    packagesToCheck = new NugetPackageEntry[0];
                } else {
                    packagesToCheck = ReadNugetPackageFromSDKProjectContents(s);
                }

                foreach (var l in mustIncludeList) {
                    bool matched = false;

                    foreach (var n in packagesToCheck) {
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

            pathToMaster = pathToMaster.Replace(MollyOptions.PRIMARYPATHLITERAL, mo.PrimaryFilePath);

            int errorCode = b.Error.Report((short)MollySubSystem.Program, (short)MollyErrorCode.ProgramCommandLineInvalidCommonDirectory, $"Path to the master files must be present and valid.  Path specified is {pathToMaster}");
            return !File.Exists(pathToMaster) ? throw new FileNotFoundException($"Error {errorCode}.  Path to master files is missing.", pathToMaster) : pathToMaster;
        }

        private void AddNugetBannedPackageAction(string ruleName, string pattern, PackageReference[] prohibitedPackages) {
            pattern = ValidateActualPath(pattern);
            var fca = new MinmatchActionCheckEntity(ruleName) {
                PerformCheck = GetBannedPackageListChecker(prohibitedPackages),
                DoesMatch = new Minimatcher(pattern, o)
            };
            Actions.Add(fca);
        }

        private void AddNugetMustIncludeAction(string ruleName, string pattern, PackageReference[] mustIncludePackages) {
            pattern = ValidateActualPath(pattern);
            var fca = new MinmatchActionCheckEntity(ruleName) {
                PerformCheck = GetMustIncludeListChecker(mustIncludePackages),
                DoesMatch = new Minimatcher(pattern, o)
            };
            Actions.Add(fca);
        }
    }
}