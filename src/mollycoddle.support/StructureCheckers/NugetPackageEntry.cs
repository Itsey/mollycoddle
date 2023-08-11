using Plisky.CodeCraft;

namespace mollycoddle {

    public class NugetPackageEntry {

        public NugetPackageEntry(string packageIdentifierValue, string packageVersionValue) {
            PackageIdentifier = packageIdentifierValue;
            RawVersion = packageVersionValue;
            Version = VersionNumber.Parse(packageVersionValue);

        }

        public string PackageIdentifier { get; set; }

        public string RawVersion { get; set; }

        public VersionNumber Version { get; set; }
    }
}