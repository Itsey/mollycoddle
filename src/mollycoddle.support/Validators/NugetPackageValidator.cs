namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

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
        public List<NugetValidationCheck> valCheck = new List<NugetValidationCheck>();
        

        public NugetPackageValidator(string owningRuleName) : base(owningRuleName) {
        }

        public void AddProhibitedPackageList(string v1, params string[] v2) {
            var v = new NugetValidationCheck() {
                Pattern = v1,
                ProhibitedPackages = v2
            };
            valCheck.Add(v);
        }

        public void AddMustReferencePackageList(string v1, params string[] v2) {
            var v = new NugetValidationCheck() {
                Pattern = v1,
                MustIncludePackages = v2
            };
            valCheck.Add(v);
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
    }
}