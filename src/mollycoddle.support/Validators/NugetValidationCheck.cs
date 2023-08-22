namespace mollycoddle;

public class NugetValidationCheck {

    public NugetValidationCheck() {
        Pattern = string.Empty;
        ProhibitedPackages = new PackageReference[0];
        MustIncludePackages = new PackageReference[0];
    }

    public PackageReference[] MustIncludePackages { get; internal set; }
    public string Pattern { get; set; }
    public PackageReference[] ProhibitedPackages { get; internal set; }
}