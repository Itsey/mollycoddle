using System.Net.Http.Headers;
using Plisky.Diagnostics;

namespace mollycoddle;
public class CommonFilesFetcher(MollyOptions options, Bilge bilge) {
    private readonly Bilge b = bilge;
    private readonly MollyOptions options = options;

    public async Task<(int errorCount, string savedDirectory)> FetchCommonFilesAsync(string? primaryRoot) {
        b.Info.Log($"Starting fetch of common files using primaryRoot: '{primaryRoot}'");
        ValidateParameters(primaryRoot);
        primaryRoot = ResolvePrimaryRoot(primaryRoot!);

        string baseUrl = primaryRoot.TrimEnd('/');
        string? username = null;
        string? password = null;
        bool isNexus = false;

        if (IsNexusToken(primaryRoot)) {
            var nexusConfig = ParseNexusConfig(primaryRoot);
            isNexus = true;
            username = nexusConfig.Username;
            password = nexusConfig.Password;
            baseUrl = nexusConfig.Url.TrimEnd('/');
        }

        string[] files = [
            "common.editorconfig",
            "common.gitignore",
            "common.nuget.config"
        ];
        string[] destFiles = [
            ".editorconfig",
            ".gitignore",
            "nuget.config"
        ];

        string currentDir = Directory.GetCurrentDirectory();
        string parentDir = Directory.GetParent(currentDir)?.FullName ?? currentDir;
        string? tempFolder = null;
        bool isSolutionRoot = Directory.GetFiles(currentDir, "*.sln").Length > 0;
        if (!isSolutionRoot) {
            tempFolder = Path.Combine(Path.GetTempPath(), "primaryfiles");
            if (!Directory.Exists(tempFolder)) {
                Directory.CreateDirectory(tempFolder);
            }
        }

        string savedDirectory = isSolutionRoot ? currentDir : tempFolder ?? currentDir;
        b.Info.Log($"Fetching common files from '{baseUrl}'.");
        int errorCount = 0;
        for (int i = 0; i < files.Length; i++) {
            string sourceUrl = $"{baseUrl}/{files[i]}";
            string destFolder = GetDestinationFolder(i, isSolutionRoot, currentDir, parentDir, tempFolder);
            string destPath = Path.Combine(destFolder, destFiles[i]);
            try {
                b.Info.Log($"Downloading '{files[i]}'...");
                await DownloadFile(sourceUrl, destPath, isNexus, username, password);
            } catch (Exception ex) {
                MollyError.Throw(ErrorModule.NexusModule, ErrorCode.NexusMarkerNotFound, $"Failed to download '{files[i]}' from '{sourceUrl}' to '{destPath}': {ex.Message}");
            }
        }
        b.Info.Log($"Common files fetch completed with {errorCount} errors.");
        return (errorCount, savedDirectory);
    }

    private static void ValidateParameters(string? primaryRoot) {
        if (string.IsNullOrWhiteSpace(primaryRoot)) {
            MollyError.Throw(ErrorModule.NexusModule, ErrorCode.NexusMarkerNotFound, "Repository root and Primary root (base URL) must be specified for get command.");
        }
    }

    private static string ResolvePrimaryRoot(string primaryRoot) {
        if (primaryRoot.StartsWith("%NEXUSCONFIG%", StringComparison.OrdinalIgnoreCase)) {
            string envValue = Environment.GetEnvironmentVariable("NEXUSCONFIG") ?? string.Empty;
            return string.Concat(envValue, primaryRoot.AsSpan("%NEXUSCONFIG%".Length));
        }
        return primaryRoot;
    }

    private static bool IsNexusToken(string primaryRoot) {
        return primaryRoot.StartsWith("[NEXUS]", StringComparison.OrdinalIgnoreCase);
    }

    private NexusConfig ParseNexusConfig(string primaryRoot) {
        var nexusSupport = new NexusSupport(options);
        var nexusConfig = nexusSupport.GetNexusSettings(primaryRoot);
        if (nexusConfig == null) {
            MollyError.Throw(ErrorModule.NexusModule, ErrorCode.NexusMarkerNotFound, "Failed to parse Nexus token from primaryRoot.");
        }
        return nexusConfig!;
    }

    private static string GetDestinationFolder(int fileIndex, bool isSolutionRoot, string currentDir, string parentDir, string? tempFolder) {
        string destFolder = (fileIndex == 1) ? parentDir : currentDir; // .gitignore goes to parent, others to current
        if (!isSolutionRoot && tempFolder != null) {
            destFolder = tempFolder;
        }
        return destFolder;
    }

    private static async Task DownloadFile(string sourceUrl, string destPath, bool isNexus, string? username, string? password) {
        if (!Uri.TryCreate(sourceUrl, UriKind.Absolute, out var uriResult)) {
            MollyError.Throw(ErrorModule.NexusModule, ErrorCode.NexusMarkerNotFound, $"Source URL '{sourceUrl}' is not a valid absolute URI. Skipping download.");
        }
        using var client = new HttpClient();
        if (isNexus && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
        byte[] fileBytes = await client.GetByteArrayAsync(sourceUrl);
        File.WriteAllBytes(destPath, fileBytes);
    }
}

