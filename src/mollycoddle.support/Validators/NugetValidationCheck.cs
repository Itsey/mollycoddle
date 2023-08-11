using Plisky.CodeCraft;

namespace mollycoddle {

    public class PackageReference {
        public string PackageName { get; set; }
        public bool AllVersions { get; set; }
        public VersionNumber? LowNumber { get; set; }
        public VersionNumber? HighNumber { get; set; }

        public PackageReference() {
            PackageName = string.Empty;
            AllVersions = true;
        }
    }


    public class NugetValidationCheck {

        public string Pattern { get; set; }
        public PackageReference[] ProhibitedPackages { get; internal set; }

        public PackageReference[] MustIncludePackages { get; internal set; }

        public NugetValidationCheck() {
            Pattern = string.Empty;
            ProhibitedPackages = new PackageReference[0];
            MustIncludePackages = new PackageReference[0];
        }
    }
}