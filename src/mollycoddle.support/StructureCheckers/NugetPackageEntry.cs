namespace mollycoddle {

    public class NugetPackageEntry {

        public NugetPackageEntry(string packageIdentifierValue, string packageVersionValue) {
            PackageIdentifier = packageIdentifierValue;
            Version = packageVersionValue;
        }

        public string PackageIdentifier { get; set; }

        public string Version { get; set; }
    }
}