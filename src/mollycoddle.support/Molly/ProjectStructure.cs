// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using Plisky.Diagnostics;

public class ProjectStructure {
    private Bilge b = new Bilge("mc-projectstructure");

    public ProjectStructure() {
        Root = string.Empty;
        AllFolders = new List<string>();
        AllFiles = new List<string>();
    }

    public List<string> AllFiles { get; set; }
    public List<string> AllFolders { get; set; }
    public string Root { get; set; }

    public List<string> GetAllDirectories(string path, string searchPattern = "*") {
        b.Verbose.Log($"Working on {path}");

        string[] directoryList = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
        var dir = new List<string>(directoryList);

        foreach (string directoryToCheck in directoryList) {
            if (directoryToCheck.EndsWith("\\.git") || directoryToCheck.EndsWith("\\.vs")) {
                continue;
            }
            dir.AddRange(GetAllDirectories(directoryToCheck, searchPattern));
        }

        return dir;
    }

    /// <summary>
    /// Returns true if the file exists, false otherwise.  This is used to abstract out File.Exists for simpler
    /// unit testing.
    /// </summary>
    /// <param name="filename">The filename to check for whether it exists.</param>
    /// <returns>true if the file exists, false otherwise.</returns>
    public virtual bool DoesFileExist(string filename) {
        // Method used to abstract the file system so mock project structure can replace these methods
        return File.Exists(filename);
    }

    /// <summary>
    /// Gets a file contents by reading it from the disk, returns null if the file isnt found.
    /// </summary>
    /// <param name="filename">The filename to read the contents from</param>
    /// <returns>The contents of the file, or null if the file is not found</returns>
    public virtual string? GetFileContents(string filename) {
        // Method is used so that the mock project structure can remove the dependency on the file system.
        if (!DoesFileExist(filename)) { return null;  }

        return File.ReadAllText(filename);
    }
    public virtual Tuple<long, byte[]> GetFileHashAndLength(string masterContentsPath) {
        // Method used to abstract the file system so mock project structure can replace these methods
        var fi = new FileInfo(masterContentsPath);
        using var fs = fi.OpenRead();
        return new Tuple<long, byte[]>(fi.Length, MD5.Create().ComputeHash(fs));
    }

    public void PopulateProjectStructure() {
        b.Verbose.Log("Getting Directories");
        var directoriesInRoot = GetAllDirectories(Root);

        foreach (string topLevelDirectory in directoriesInRoot) {
            AllFolders.Add(topLevelDirectory.ToLowerInvariant());
        }

        b.Verbose.Log($"{AllFolders.Count} folders, now getting files");

        var allFilesUnderRoot = Directory.EnumerateFileSystemEntries(Root, "*.*", SearchOption.AllDirectories);
        foreach (string nextFile in allFilesUnderRoot) {
            AllFiles.Add(nextFile.ToLowerInvariant());
        }

        b.Verbose.Log($"{AllFiles.Count} files");
    }

  
}