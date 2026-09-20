using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Plisky.Diagnostics;

namespace mollycoddle;

public class NexusSupport {
    public const string ALT_INDEX_FILENAME = "mccache.index";
    public const string INDEX_FILENAME = "index.json";
    public const string NEXUS_PREFIX = "[NEXUS]";
    protected const int MAXCONCURRENTDOWNLOADS = 16;  // Note AI suggestion of average for small downloads.
    protected const int MAXRETRIES = 2;
    protected const int RETRYDELAYMS = 1000;
    protected const int SHARINGVIOLATIONHRESULT = unchecked((int)0x80070020);

    protected readonly MollyOptions mo;

    protected Bilge b = new("molly-nexus");

    private static readonly HttpClient client = new();

    private static readonly string[] knownNexusChunkMarkers = new[] { "[U::", "[P::", "[L::", "[R::", "[G::" };

    public NexusSupport(MollyOptions mox) {
        mo = mox;
    }

    public string? BasePathToSave { get; set; }

    protected static JsonSerializerOptions Opts { get; } = new JsonSerializerOptions {
        WriteIndented = true
    };

    /// <summary>
    ///     This takes the formatted filenames for nexus and turns it into a local filename by parsing out the sections of the
    ///     nexus
    ///     path that hold the group names.
    /// </summary>
    /// <param name="fileContents">The raw data to write to the file.</param>
    /// <param name="filenameWithRulesPathing">
    ///     The full path in the nexus repo e.g. /molly/default/filename.xtn this must be a
    ///     full path.
    /// </param>
    /// <param name="ruleGroupName">The group name which is the start of the full path e.g. /molly.</param>
    public void ActualSaver(byte[] fileContents, string filenameWithRulesPathing, string ruleGroupName) {
        if (BasePathToSave == null) {
            MollyError.Throw(ErrorModule.NexusModule, ErrorCode.NexusMarkerNotFound, "Base path to save has not been set.");
        }
        var parts = GetVersionAndFilenameFromNexusUrl(ruleGroupName, filenameWithRulesPathing);
        string relativePath = parts.Item2.Replace('/', Path.DirectorySeparatorChar);
        string localFile = Path.Combine(BasePathToSave, parts.Item1, relativePath);

        string? targetDir = Path.GetDirectoryName(localFile);
        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir)) {
            Directory.CreateDirectory(targetDir);
        }

        PhysicallyWriteFile(fileContents, localFile);
    }

    public async Task CacheNexusFiles(NexusConfig nc, string identifier, Action<byte[], string, string> saveFile) {
        b.Info.Flow($"{identifier}");

        if (string.IsNullOrEmpty(nc.Repository)) {
            throw new InvalidOperationException(
                $"Unable to connect to Nexus ({nc.Server}) correctly - missing repository.");
        }

        if (BasePathToSave == null) {
            MollyError.Throw(ErrorModule.NexusModule, ErrorCode.NexusMarkerNotFound, "Base path to save has not been set.");
        }

        string purl = GetUrlToUse(nc.Url, identifier);
        var pmr = GetVersionAndFilenameFromNexusUrl(identifier, purl);

        string groupFolder = !string.IsNullOrEmpty(pmr.Item1)
            ? Path.Combine(BasePathToSave, pmr.Item1)
            : BasePathToSave;

        if (!Directory.Exists(groupFolder)) {
            Directory.CreateDirectory(groupFolder);
        }

        var currentIndex = LoadIndex(groupFolder);

        var allMatchingAssets = new List<NexusAssetItem>();
        var filesToDownload = new List<NexusAssetItem>();
        AuthenticationHeaderValue? authHeader = null;

        if (!string.IsNullOrEmpty(nc.Username)) {
            b.Verbose.Log($"MC-Nexus > Adding Authentication to Nexus Request - {nc.Username}");
            byte[] byteArray = new UTF8Encoding().GetBytes($"{nc.Username}:{nc.Password}");
            authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        string? continuationToken = null;

        try {
            do {
                string assetsApi = $"{nc.Server}/service/rest/v1/assets?repository={nc.Repository}";
                if (!string.IsNullOrEmpty(continuationToken)) {
                    assetsApi += $"&continuationToken={Uri.EscapeDataString(continuationToken)}";
                }

                b.Verbose.Log($"MC-Nexus > Attempting to connect to {assetsApi}");
                var request = new HttpRequestMessage(HttpMethod.Get, assetsApi);

                if (authHeader != null) {
                    request.Headers.Authorization = authHeader;
                }

                b.Verbose.Log("About to send request to list all assets.");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                b.Verbose.Log($"MC-Nexus > Nexus, List All Files > Content received {content.Length}");
                using var jsonDocument = JsonDocument.Parse(content);
                var root = jsonDocument.RootElement;

                if (root.TryGetProperty("items", out var itemsElement) &&
                    itemsElement.ValueKind == JsonValueKind.Array) {
                    foreach (var item in itemsElement.EnumerateArray()) {
                        if (item.TryGetProperty("path", out var pathElement)) {
                            if (item.TryGetProperty("id", out var idElement)) {
                                b.Verbose.Log($"MC-Nexus > Nexus File Identified : {pathElement} {idElement}");
                                string? nexusPathValue = pathElement.GetString();
                                string? nexusIdValue = idElement.GetString();

                                if (nexusPathValue == null || nexusIdValue == null) {
                                    b.Warning.Log("The pathElement is invalid, can not parse this path element");
                                    throw new NullReferenceException("Nexus file path is null");
                                }

                                string normalizedPath = nexusPathValue.StartsWith('/')
                                    ? nexusPathValue
                                    : "/" + nexusPathValue;
                                string normalizedIdentifier =
                                    identifier.StartsWith('/') ? identifier : "/" + identifier;

                                var thismr =
                                    GetVersionAndFilenameFromNexusUrl(normalizedIdentifier, normalizedPath);

                                if (normalizedPath.StartsWith(
                                        normalizedIdentifier,
                                        StringComparison.OrdinalIgnoreCase) &&
                                    string.Equals(thismr.Item1, pmr.Item1, StringComparison.OrdinalIgnoreCase) &&
                                    !string.IsNullOrEmpty(thismr.Item2)) {
                                    string? sha1 = null;
                                    if (item.TryGetProperty("checksum", out var checksumElement) &&
                                        checksumElement.TryGetProperty("sha1", out var sha1Element)) {
                                        sha1 = sha1Element.GetString();
                                    }

                                    string relativePath = thismr.Item2.Replace('/', Path.DirectorySeparatorChar);
                                    string localFile = Path.Combine(groupFolder, relativePath);

                                    var assetItem = new NexusAssetItem(normalizedPath, nexusIdValue, thismr.Item2, sha1);
                                    allMatchingAssets.Add(assetItem);

                                    bool needsDownload = true;
                                    if (!string.IsNullOrEmpty(sha1) && File.Exists(localFile)) {
                                        if (currentIndex.TryGetValue(thismr.Item2, out string? cachedSha1)) {
                                            if (string.Equals(cachedSha1, sha1, StringComparison.OrdinalIgnoreCase)) {
                                                needsDownload = false;
                                            }
                                        }
                                    }

                                    b.Verbose.Log($"MC-Nexus > Queuing File For Local Cache : {normalizedPath} : {needsDownload}");
                                    if (needsDownload) {
                                        filesToDownload.Add(assetItem);
                                    }
                                }
                            }
                        }
                    }
                }

                if (root.TryGetProperty("continuationToken", out var tokenElement) &&
                    tokenElement.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(tokenElement.GetString())) {
                    continuationToken = tokenElement.GetString();
                } else {
                    continuationToken = null;
                }
            } while (!string.IsNullOrEmpty(continuationToken));
        } catch (HttpRequestException hrx) {
            throw new InvalidOperationException($"Unable to connect to Nexus ({nc.Server}) correctly. Status:{hrx.StatusCode}", hrx);
        }

        b.Info.Log($"MC-Nexus > {allMatchingAssets.Count} matching files found on Nexus for cache {identifier}");

        if (allMatchingAssets.Count == 0) {
            throw new InvalidOperationException($"Nexus ({nc.Server}) is incorrect. No files were found to retrieve. Check {nc.Url}");
        }

        if (filesToDownload.Count == 0) {
            b.Info.Log($"MC-Nexus > All {allMatchingAssets.Count} files for {identifier} are up to date in cache.");
            return;
        }

        b.Info.Log($"MC-Nexus > Downloading {filesToDownload.Count} of {allMatchingAssets.Count} files for cache {identifier}");

        var parallelOptions = new ParallelOptions {
            MaxDegreeOfParallelism = MAXCONCURRENTDOWNLOADS
        };

        await Parallel.ForEachAsync(filesToDownload, parallelOptions, async (l, ct) => {
            string relativePath = l.NormalizedPath.StartsWith('/') ? l.NormalizedPath : "/" + l.NormalizedPath;
            string downloadPath = $"{nc.Server}/repository/{nc.Repository}{relativePath}";
            b.Info.Log($"MC-Nexus > Caching Local Copy : {downloadPath}");
            await DownloadFileAsync(downloadPath, SaveCreator(saveFile, identifier), l.NormalizedPath, nc.Username, nc.Password, ct);
        });

        foreach (var downloaded in filesToDownload) {
            string? sha1ToStore = downloaded.Sha1;
            if (string.IsNullOrEmpty(sha1ToStore)) {
                string relativePath = downloaded.RelativeFilename.Replace('/', Path.DirectorySeparatorChar);
                string localFile = Path.Combine(groupFolder, relativePath);
                if (File.Exists(localFile)) {
                    byte[] fileBytes = File.ReadAllBytes(localFile);
                    sha1ToStore = Convert.ToHexString(SHA1.HashData(fileBytes)).ToLowerInvariant();
                }
            }
            if (!string.IsNullOrEmpty(sha1ToStore)) {
                currentIndex[downloaded.RelativeFilename] = sha1ToStore;
            }
        }

        SaveIndex(groupFolder, currentIndex);
    }

    public async Task DownloadFileAsync(string downloadUrl, Action<byte[], string> saveFile, string fileName, string? username, string? password, CancellationToken cancellationToken = default) {
        b.Info.Flow();
        var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) {
            byte[] byteArray = new UTF8Encoding().GetBytes($"{username}:{password}");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        var response = await client.SendAsync(request, cancellationToken);
        b.Verbose.Log("Request Made, checking response", $"{downloadUrl}");
        response.EnsureSuccessStatusCode();

        byte[] fileBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        // await File.WriteAllBytesAsync(filename, fileBytes);
        saveFile(fileBytes, fileName);
    }

    public Dictionary<string, MarkerPosition> GetChunks(string nexusUrl, string[] markers) {
        var result = new Dictionary<string, MarkerPosition>();

        var mrks = new List<MarkerPosition>();
        foreach (string nextMarker in markers) {
            var m = new MarkerPosition {
                Marker = nextMarker,
                Position = nexusUrl.IndexOf(nextMarker)
            };
            mrks.Add(m);
        }

        var mio = mrks.OrderBy(p => p.Position).ToList();
        Debug.Assert(mio != null, "mio should not be null");

        for (int i = 0; i < mio.Count; i++) {
            if (mio[i].Position < 0) {
                continue;
            }

            if (i == mio.Count - 1) {
                mio[i].Value = nexusUrl[(mio[i].Position + mio[i].Marker.Length)..];
            } else {
                int start = mio[i].Position + mio[i].Marker.Length;
                int length = mio[i + 1].Position - mio[i].Position - mio[i].Marker.Length;
                mio[i].Value = nexusUrl[start..(start + length)];
            }
        }

        foreach (var l in mrks) {
            result.Add(l.Marker, l);
        }
        return result;
    }

    [Pure]
    public NexusConfig? GetNexusSettings(string nexusToken) {
        if (!nexusToken.StartsWith(NEXUS_PREFIX)) {
            return null;
        }

        string nexusParse = nexusToken[NEXUS_PREFIX.Length..];
        var chunks = GetChunks(nexusParse, knownNexusChunkMarkers);

        string? username = chunks["[U::"].Value;
        string? password = chunks["[P::"].Value;
        string? nexusUrl = chunks["[L::"].Value;

        if (string.IsNullOrEmpty(nexusUrl)) {
            return null;
        }

        // Ensure it ends with a slash if it looks like a folder not a file.
        if (!nexusUrl.EndsWith('/')) {
            string lastSegment = nexusUrl[(nexusUrl.LastIndexOf('/') + 1)..];
            if (!Path.HasExtension(lastSegment)) {
                nexusUrl += "/";
            }
        }

        int httpPos = nexusUrl.IndexOf("://");
        int afterHttp = nexusUrl.IndexOf('/', httpPos + 3);
        string server = nexusUrl[..afterHttp];

        var result = new NexusConfig {
            Username = username,
            Password = password,
            Url = nexusUrl,
            FilenameUrl = nexusUrl[(nexusUrl.LastIndexOf('/') + 1)..],
            Server = server,
            Repository = chunks["[R::"]?.Value,
            SearchPath = chunks["[G::"]?.Value
        };
        return result;
    }

    public string GetUrlToUse(string sourceUrl, string marker) {
        int mmOffset = sourceUrl.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

        if (mmOffset < 0) {
            string altMarker = marker.StartsWith('/') ? marker.TrimStart('/') : "/" + marker;
            mmOffset = sourceUrl.IndexOf(altMarker, StringComparison.OrdinalIgnoreCase);
            if (mmOffset < 0) {
                throw new InvalidOperationException($"Nexus URL does not contain the expected marker '{marker}'");
            }
        }

        string urlToUse = sourceUrl[mmOffset..];
        if (!urlToUse.StartsWith('/')) {
            urlToUse = "/" + urlToUse;
        }

        return urlToUse;
    }

    public Tuple<string, string> GetVersionAndFilenameFromNexusUrl(string molsbaseMarker, string downloadUrl) {
        b.Info.Flow($"{downloadUrl}");

        string cleanMarker = molsbaseMarker.Trim('/');
        string cleanUrl = downloadUrl.TrimStart('/');

        if (cleanUrl.StartsWith(cleanMarker, StringComparison.OrdinalIgnoreCase)) {
            string working = cleanUrl[cleanMarker.Length..].TrimStart('/');
            int firstSlash = working.IndexOf('/');

            if (firstSlash >= 0) {
                string version = working[..firstSlash];
                string filename = working[(firstSlash + 1)..];
                return new Tuple<string, string>(version, filename);
            }
            if (!string.IsNullOrEmpty(working)) {
                return new Tuple<string, string>(working, string.Empty);
            }
        }

        b.Warning.Log($"download url did not start with marker ]{molsbaseMarker}[, returning empty");
        return new Tuple<string, string>(string.Empty, string.Empty);
    }

    public virtual Dictionary<string, string> LoadIndex(string folderPath) {
        b.Verbose.Flow($"{folderPath}");

        string primaryPath = Path.Combine(folderPath, INDEX_FILENAME);
        string altPath = Path.Combine(folderPath, ALT_INDEX_FILENAME);

        string? pathToUse = File.Exists(primaryPath) ? primaryPath : File.Exists(altPath) ? altPath : null;
        if (pathToUse == null) {
            b.Verbose.Log($"No index file found in {folderPath}");
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try {
            string json = File.ReadAllText(pathToUse);
            using var doc = JsonDocument.Parse(json);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (doc.RootElement.ValueKind == JsonValueKind.Object) {
                var target = doc.RootElement;
                if (target.TryGetProperty("files", out var filesProp) && filesProp.ValueKind == JsonValueKind.Object) {
                    target = filesProp;
                }

                foreach (var prop in target.EnumerateObject()) {
                    if (prop.Value.ValueKind == JsonValueKind.String) {
                        result[prop.Name.Replace('\\', '/')] = prop.Value.GetString() ?? string.Empty;
                    }
                }
            } else if (doc.RootElement.ValueKind == JsonValueKind.Array) {
                foreach (var item in doc.RootElement.EnumerateArray()) {
                    if (item.ValueKind == JsonValueKind.Object &&
                        item.TryGetProperty("filename", out var fn) &&
                        item.TryGetProperty("sha1", out var sh)) {
                        string? fnStr = fn.GetString();
                        string? shStr = sh.GetString();
                        if (!string.IsNullOrEmpty(fnStr) && !string.IsNullOrEmpty(shStr)) {
                            result[fnStr.Replace('\\', '/')] = shStr;
                        }
                    }
                }
            }

            b.Verbose.Log($"Loaded {result.Count} entries from index file {pathToUse}");
            return result;
        } catch (Exception ex) {
            b.Warning.Log($"Unable to parse index file at {pathToUse}. Treating as empty. Exception: {ex.Message}");
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public virtual void PhysicallyWriteFile(byte[] fileContents, string localFile) {
        int count = 0;

        while (true) {
            try {
                if (File.Exists(localFile)) {
                    byte[] existingContents = File.ReadAllBytes(localFile);
                    if (existingContents.AsSpan().SequenceEqual(fileContents)) {
                        b.Verbose.Log($"MC-Nexus > Skipping write, contents identical: {localFile}");
                        return;
                    }
                }
                File.WriteAllBytes(localFile, fileContents);
                return;
            } catch (IOException ex) when (ex.HResult == SHARINGVIOLATIONHRESULT) {
                count++;
                b.Action.Occured("retry", $"count {count}");
                b.Warning.Log($"NexusCache file {localFile} in use.  Try {count} of {MAXRETRIES}.");
                if (count >= MAXRETRIES) {
                    b.Error.Log(
                        $"NexusCache file {localFile} still in use after {MAXRETRIES} attempts. Exception: {ex.Message}");
                    throw;
                }
                Task.Delay(RETRYDELAYMS).Wait();
            }
        }
    }

    public async Task<string> ProcessNexusSupport(string nexusFile, ProcessKind fileType) {
        b.Info.Flow($"{nexusFile}");

        Action<byte[], string, string> saveFileAction = (fileContents, fileName, identifier) => { ActualSaver(fileContents, fileName, identifier); };

        string result = nexusFile;
        var ns = GetNexusSettings(nexusFile);
        if (string.IsNullOrEmpty(nexusFile) || !nexusFile.StartsWith(NEXUS_PREFIX) || ns == null) {
            return result;
        }

        string nexusMollyMarker = fileType == ProcessKind.RulesFile ? "/molly" : "/primaryfiles";
        string urlToUse = GetUrlToUse(ns.Url, nexusMollyMarker);

        await CacheNexusFiles(ns, nexusMollyMarker, saveFileAction);
        (string version, string filename) = GetVersionAndFilenameFromNexusUrl(nexusMollyMarker, urlToUse);
        if (string.IsNullOrEmpty(BasePathToSave)) {
            throw new InvalidOperationException("BasePath must be set before the file saves are processed.");
        }

        if (fileType == ProcessKind.RulesFile) {
            result = Path.Combine(BasePathToSave, version, filename);
        } else {
            result = Path.Combine(BasePathToSave, version);
        }

        return result;
    }

    public virtual void SaveIndex(string folderPath, Dictionary<string, string> index) {
        b.Verbose.Flow($"{folderPath}");

        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
        }

        string indexPath = Path.Combine(folderPath, INDEX_FILENAME);
        try {
            var sorted = index.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key.Replace('\\', '/'), v => v.Value, StringComparer.OrdinalIgnoreCase);

            string json = JsonSerializer.Serialize(sorted, Opts);
            File.WriteAllText(indexPath, json);
            b.Verbose.Log($"Saved {sorted.Count} entries to index file {indexPath}");
        } catch (Exception ex) {
            b.Error.Log($"Unable to save index file to {indexPath}. Exception: {ex.Message}");
        }
    }

    public async Task UploadFileAsync(Stream fileContent, string repositoryPath, string? username, string? password) {
        using var content = new StreamContent(fileContent);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var request = new HttpRequestMessage(HttpMethod.Put, repositoryPath) {
            Content = content
        };

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) {
            byte[] byteArray = new UTF8Encoding().GetBytes($"{username}:{password}");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private Action<byte[], string> SaveCreator(Action<byte[], string, string> saver, string path) {
        return (b, c) => saver(b, c, path);
    }
}