using Plisky.CodeCraft;

namespace mollycoddle;

public class NugetPackageEntry(string packageIdentifierValue, string packageVersionValue) {
    public string PackageIdentifier { get; set; } = packageIdentifierValue;

    public string RawVersion { get; set; } = packageVersionValue;

    public VersionNumber Version { get; set; } = VersionNumber.Parse(packageVersionValue);
}