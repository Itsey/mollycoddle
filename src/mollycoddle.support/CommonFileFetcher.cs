using Plisky.Diagnostics;

namespace mollycoddle;

public class CommonFilesFetcher(MollyOptions options, Bilge bilge) {
    private readonly Bilge b = bilge;
    private readonly MollyOptions mo = options;
    private readonly List<PrimaryCopyFile> commonFileMappings = [];

    public int FetchCommonFiles() {
        b.Info.Flow();
        int errorCount = 0;
        string commonPath = mo.PrimaryFilePath!;
        string targetDir = mo.DirectoryToTarget;

        ValidateParameters(commonPath, mo.RulesFile);

        try {
            b.Info.Log($"Starting fetch of common files");
            LoadCommonFileMappings();
        } catch (Exception ex) {
            errorCount++;
            b.Error.Log($"Failed to load rules to determine common files: {ex.Message}");
            return errorCount;
        }
        if (commonFileMappings.Count == 0) {
            b.Warning.Log("No common files to fetch based on the provided rules.");
            return -1;
        }

        foreach (var mapping in commonFileMappings) {
            if (string.IsNullOrWhiteSpace(mapping.FullPathForCommonFile) || string.IsNullOrWhiteSpace(mapping.PatternForSourceFile)) {
                errorCount++;
                b.Warning.Log("Skipping invalid common file mapping with empty paths.");
                continue;
            }
            string commonFileName = Path.GetFileName(mapping.FullPathForCommonFile);
            string destPattern = mapping.PatternForSourceFile
                .Replace("%ROOT%", targetDir)
                .Replace(MollyOptions.PRIMARYPATHLITERAL, commonPath);
            //TODO: PrimaryPathLiteral replacement should be with ProjectStructure
            //TODO: Add handling for when destPattern contains wildcards or other complex patterns
            string? destDir = Path.GetDirectoryName(destPattern);
            string? destFileName = Path.GetFileName(destPattern);
            if (string.IsNullOrWhiteSpace(destFileName)) {
                destFileName = commonFileName; //If the pattern provided no filename, fallback to derive from source file name
            }

            string? cachedFile = Directory.GetFiles(commonPath, commonFileName, SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(cachedFile)) {
                errorCount++;
                b.Warning.Log($"Common file '{commonFileName}' not found in '{commonPath}'.");
                continue; // Skip to next file if not found
            }

            int destDirErrorCount = EnsureDestinationDirectoryExists(destDir, destPattern);
            if (destDirErrorCount > 0) {
                errorCount += destDirErrorCount;
                continue; // Skip to next file if destination directory is invalid
            }

            string destPath = Path.Combine(destDir!, destFileName);
            errorCount += CopyCommonFiles(cachedFile, destPath, destFileName);
        }
        b.Info.Log($"Common files fetch completed with {errorCount} errors.");
        return errorCount;
    }

    private int EnsureDestinationDirectoryExists(string? destDir, string destPattern) {
        b.Info.Flow();
        int errorCount = 0;
        try {
            if (string.IsNullOrWhiteSpace(destDir)) {
                errorCount++;
                b.Error.Log($"Destination folder could not be determined for pattern '{destPattern}'.");
                return errorCount;
            }
            if (!Directory.Exists(destDir)) {
                Directory.CreateDirectory(destDir);
            }
        } catch (Exception dex) {
            errorCount++;
            b.Error.Log($"Failed to create destination directory '{destDir}': {dex.Message}");
            return errorCount;
        }
        return errorCount;
    }

    private void LoadCommonFileMappings() {
        // TODO: Review whether this has already been done in a prior step, consider passing in the data instead of reloading
        // Load rules and extract any common files referenced in AdditionalData along with their pattern match
        b.Info.Flow();
        var mrf = new MollyRuleFactory();
        foreach (var r in mrf.LoadRulesFromFile(mo.RulesFile)) {
            foreach (var v in r.Validators) {
                if (v is FileValidator fv) {
                    foreach (var p in fv.FilesThatMustMatchTheirCommon()) {
                        if (!string.IsNullOrWhiteSpace(p.FullPathForCommonFile) && !string.IsNullOrWhiteSpace(p.PatternForSourceFile)) {
                            commonFileMappings.Add(new PrimaryCopyFile(p.PatternForSourceFile, p.FullPathForCommonFile));
                        }
                    }
                }
            }
        }
    }

    private static void ValidateParameters(string? commonRoot, string rulesFile) {
        // TODO: Consider using Validation in FileStuctureChecker
        if (string.IsNullOrWhiteSpace(commonRoot) || !Directory.Exists(commonRoot)) {
            MollyError.Throw(ErrorModule.MollyOptions, ErrorCode.ProgramCommandLineInvalidCommonDirectory, "Primary root (base URL) must be specified for get command.");
        }
        if (string.IsNullOrWhiteSpace(rulesFile) || !File.Exists(rulesFile)) {
            MollyError.Throw(ErrorModule.RulesFiles, ErrorCode.ProgramCommandLineRulesFileMissing, "Rules file must be specified for get command.");
        }
    }

    private int CopyCommonFiles(string cachedFile, string destPath, string commonFileName) {
        b.Info.Flow();
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