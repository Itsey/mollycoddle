namespace mollycoddle;

using Plisky.CodeCraft;

public class PackageReference {

    public PackageReference() {
        PackageName = string.Empty;
        VersionMatchType = PackageVersionMatchType.AllVersions;
    }

    public VersionNumber? HighVersionNumber { get; set; }
    public VersionNumber? LowVersionNumber { get; set; }
    public string PackageName { get; set; }
    public PackageVersionMatchType VersionMatchType { get; set; }
}