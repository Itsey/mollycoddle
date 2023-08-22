namespace mollycoddle.test;

internal class NugetPackageValidatorInternals : NugetPackageValidator {

    public NugetPackageValidatorInternals(string owningRuleName) : base(owningRuleName) {
    }

    public PackageReference ParsePackageListStringToPackageReference_Test(string packageString) {
        return ParsePackageListStringToPackageReference(packageString);
    }
}