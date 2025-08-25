using Nuke.Common.IO;

public class LocalBuildConfig {
    public AbsolutePath ArtifactsDirectory { get; set; }
    public bool NonDestructive { get; set; } = true;
    public string VersioningPersistanceTokenPre { get; set; }
    public string MainProjectName { get; internal set; }
    public AbsolutePath DependenciesDirectory { get; internal set; }
    public object ExecutingMachineName { get; internal set; }
    public string VersioningPersistanceTokenRelease { get; internal set; }
    public string MollyRulesToken { get; set; }
    public string MollyPrimaryToken { get; set; }
    public string MollyRulesVersion { get; set; }
}