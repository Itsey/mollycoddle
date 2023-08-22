namespace mollycoddle;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Plisky.CodeCraft;
using Plisky.Diagnostics;

/// <summary>
/// The job here is to describe the validations technically so that the checker can execute them but this class should not
/// know how to do the validations just what the validations must be.
/// </summary>
[DebuggerDisplay("Validator For {TriggeringRule}")]
public class NugetPackageValidator : ValidatorBase {

    /// <summary>
    /// This is the name of the validator, it must be specified exactly in the rules files.  It does not use nameof to prevent accidental refactoring.
    /// </summary>
    public const string VALIDATORNAME = "NugetValidationChecks";

    public List<NugetValidationCheck> valCheck = new();

    public NugetPackageValidator(string owningRuleName) : base(owningRuleName) {
        b = new Bilge("mc-validators-nuget");
    }

    public void AddMustReferencePackageList(string filenamePatternToMatch, params string[] mustReferencePackageList) {
        List<PackageReference> refs = new();

        foreach (string f in mustReferencePackageList) {
            refs.Add(new PackageReference() {
                PackageName = f,
                VersionMatchType = PackageVersionMatchType.AllVersions
            });
        }

        var v = new NugetValidationCheck() {
            Pattern = filenamePatternToMatch,
            MustIncludePackages = refs.ToArray()
        };
        valCheck.Add(v);
    }

    public void AddProhibitedPackageList(string filenamePatternToMatch, params string[] prohibitedPackageList) {
        List<PackageReference> refs = new();

        foreach (string f in prohibitedPackageList) {
            var reference = ParsePackageListStringToPackageReference(f);
            refs.Add(reference);
        }

        var v = new NugetValidationCheck() {
            Pattern = filenamePatternToMatch,
            ProhibitedPackages = refs.ToArray()
        };
        valCheck.Add(v);
    }

    /// <summary>
    /// Returns one or more rules that highlight prohibited packages against match names.  E.g. *.csproj, bannedPackages[]
    /// </summary>
    /// <returns></returns>
    public IEnumerable<NugetValidationCheck> GetMustReferencePackagesList() {
        foreach (var f in valCheck) {
            if (f.MustIncludePackages.Any()) {
                yield return f;
            }
        }
    }

    /// <summary>
    /// Returns one or more rules that highlight prohibited packages against match names.  E.g. *.csproj, bannedPackages[]
    /// </summary>
    /// <returns></returns>
    public IEnumerable<NugetValidationCheck> GetProhibitedPackagesLists() {
        foreach (var f in valCheck) {
            if (f.ProhibitedPackages.Any()) {
                yield return f;
            }
        }
    }

    protected virtual PackageReference ParsePackageListStringToPackageReference(string nugetPackageString) {
        if (string.IsNullOrEmpty(nugetPackageString)) {
            throw new ArgumentNullException(nameof(nugetPackageString));
        }

        var result = new PackageReference();

        if (nugetPackageString.Contains("[")) {
            string[] parts = nugetPackageString.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) {
                b.Warning.Log($"Package String [{nugetPackageString}] is corrupt, only using the first part as the package name.");
                result.PackageName = parts[0];
                result.VersionMatchType = PackageVersionMatchType.AllVersions;
            } else {
                string versionText = parts[1];
                result.PackageName = parts[0];

                if (versionText.Contains('-')) {
                    string[] subParts = versionText.Split('-');
                    var low = VersionNumber.Parse(subParts[0]);
                    var high = VersionNumber.Parse(subParts[1]);
                    result.HighVersionNumber = high;
                    result.LowVersionNumber = low;
                    result.VersionMatchType = PackageVersionMatchType.RangeProhibited;
                } else {
                    bool greaterThanActive = false;
                    bool lessThanActive = false;

                    if (versionText.StartsWith('>')) {
                        versionText = versionText[1..];
                        greaterThanActive = true;
                    } else if (versionText.StartsWith('<')) {
                        versionText = versionText[1..];
                        lessThanActive = true;
                    }

                    var ver = VersionNumber.Parse(versionText);

                    if (lessThanActive) {
                        result.LowVersionNumber = ver;
                        result.HighVersionNumber = new VersionNumber(0, 0, 0, 0);
                        result.VersionMatchType = PackageVersionMatchType.NotLessThan;
                    } else if (greaterThanActive) {
                        result.HighVersionNumber = ver;
                        result.LowVersionNumber = new VersionNumber(0, 0, 0, 0);
                        result.VersionMatchType = PackageVersionMatchType.NotMoreThan;
                    } else {
                        result.LowVersionNumber = ver;
                        result.HighVersionNumber = ver;
                        result.VersionMatchType = PackageVersionMatchType.Exact;
                    }
                }
            }
        } else {
            result.PackageName = nugetPackageString;
            result.VersionMatchType = PackageVersionMatchType.AllVersions;
        }

        return result;
    }
}