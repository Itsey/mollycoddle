// See https://aka.ms/new-console-template for more information

using Plisky.Diagnostics;

public class ProjectStructure {
    Bilge b = new Bilge("mc-projectstructure");

    public string Root { get; set; }

    public List<string> AllFolders { get; set; }
    public List<string> AllFiles { get; set; }

    public List<string> GetAllDirectories(string path, string searchPattern = "*") {
        b.Verbose.Log($"Working on {path}");

        var d = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
        var dir = new List<string>(d);


        foreach (var dl in d) {
            if (dl.EndsWith("\\.git") || dl.EndsWith("\\.vs")) {
                continue;
            }
            dir.AddRange(GetAllDirectories(dl, searchPattern));
        }

        return dir;
    }


    public void PopulateProjectStructure() {

        b.Verbose.Log("Getting Directories");
        var dirs = GetAllDirectories(Root);

        foreach (var l in dirs) {
            AllFolders.Add(l.ToLowerInvariant());
        }

        b.Verbose.Log($"{AllFolders.Count} folders, now getting files");

        var fls = Directory.EnumerateFileSystemEntries(Root, "*.*", SearchOption.AllDirectories);
        foreach (var f in fls) {
            AllFiles.Add(f.ToLowerInvariant());
        }

        b.Verbose.Log($"{AllFiles.Count} files");
    }

    public ProjectStructure() {
        Root = string.Empty;
        AllFolders = new List<string>();
        AllFiles = new List<string>();

    }

}