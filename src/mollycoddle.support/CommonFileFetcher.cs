using Plisky.Diagnostics;

namespace mollycoddle;

public class CommonFilesFetcher(MollyOptions options, Bilge bilge) {
    private readonly Bilge b = bilge;
    private readonly MollyOptions mo = options;
    private readonly string[] commonFileNames = [
        "common.editorconfig",
        "common.gitignore",
        "common.nuget.config"
        ];
    private readonly string[] destFileNames = [
        ".editorconfig",
        ".gitignore",
        "nuget.config"
        ];

    public (int errorCount, string savedDirectory) FetchCommonFiles() {
        ValidateParameters(mo.PrimaryFilePath);
        string commonPath = mo.PrimaryFilePath!;
        b.Info.Log($"Starting fetch of common files from: '{commonPath}'");

        string parentDir = mo.DirectoryToTarget;
        string srcDir = Path.Combine(parentDir, "src");
        bool isSolutionRoot = Directory.Exists(srcDir) && Directory.GetFiles(srcDir, "*.sln").Length > 0;

        int errorCount = 0;
        for (int i = 0; i < commonFileNames.Length; i++) {
            string destFileName = destFileNames[i];
            string commonFileName = commonFileNames[i];

            string? cachedFile = Directory.GetFiles(commonPath, commonFileName, SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(cachedFile)) {
                errorCount++;
                b.Warning.Log($"Common file '{commonFileName}' not found in '{commonPath}'.");
                continue; // Skip to next file if not found
            }
            string destFolder = GetDestinationFolder(i, isSolutionRoot, srcDir, parentDir);
            string destPath = Path.Combine(destFolder, destFileName);
            errorCount += CopyCommonFiles(cachedFile, destPath, commonFileName);
        }
        b.Info.Log($"Common files fetch completed with {errorCount} errors.");
        string targetDirectory = isSolutionRoot ? srcDir : parentDir;
        return (errorCount, targetDirectory);
    }

    private static void ValidateParameters(string? primaryRoot) {
        if (string.IsNullOrWhiteSpace(primaryRoot) || !Directory.Exists(primaryRoot)) {
            MollyError.Throw(ErrorModule.NexusModule, ErrorCode.NexusMarkerNotFound, "Repository root and Primary root (base URL) must be specified for get command.");
        }
    }

    private static string GetDestinationFolder(int fileIndex, bool isSolutionRoot, string srcDir, string parentDir) {
        if (!isSolutionRoot) {
            return parentDir;  // If not solution root, always use parentDir
        }
        return (fileIndex == 1) ? parentDir : srcDir; // .gitignore (index 1) goes to parentDir, others to srcDir
    }

    private int CopyCommonFiles(string cachedFile, string destPath, string commonFileName) {
        int errorCount = 0;
        try {
            File.Copy(cachedFile, destPath, overwrite: true);
            b.Info.Log($"Copied '{commonFileName}' to '{destPath}'.");
        } catch (Exception ex) {
            errorCount++;
            b.Error.Log($"Failed to copy '{commonFileName}' to '{destPath}': {ex.Message}");
        }
        return errorCount;
    }
}
