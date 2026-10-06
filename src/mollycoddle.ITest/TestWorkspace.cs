using System.Text.Json;

namespace mollycoddle.ITest;

internal sealed record TestRule(
    string Name,
    string Control,
    string Pattern,
    string ValidatorName = "FileValidationChecks",
    string[]? AdditionalData = null);

internal sealed class TestWorkspace : IDisposable {
    public string Root { get; } = Path.Combine(Path.GetTempPath(), $"mc-itest-{Guid.NewGuid():N} with spaces");
    public string Repository => Path.Combine(Root, "repository");
    public string RulesDirectory => Path.Combine(Root, "rules");
    public string PrimaryDirectory => Path.Combine(Root, "primary");
    public string CacheDirectory => Path.Combine(Root, "cache");

    public TestWorkspace() {
        Directory.CreateDirectory(Repository);
        Directory.CreateDirectory(RulesDirectory);
        Directory.CreateDirectory(PrimaryDirectory);
        Directory.CreateDirectory(CacheDirectory);
    }

    public string WriteFile(string relativePath, string content = "fixture contents") {
        return Write(Path.Combine(Repository, relativePath), content);
    }

    public void CreateDirectory(string relativePath) {
        Directory.CreateDirectory(Path.Combine(Repository, relativePath));
    }

    public string WritePrimary(string relativePath, string content) {
        return Write(Path.Combine(PrimaryDirectory, relativePath), content);
    }

    public string WriteRules(params TestRule[] rules) => WriteRulesFile("rules.molly", rules);

    public string WriteRulesFile(string fileName, params TestRule[] rules) {
        var document = new {
            RulesetName = "Integration characterization",
            Rules = rules.Select(rule => new {
                rule.Name,
                RuleReference = rule.Name,
                Link = "https://example.invalid/integration-rule",
                Validators = new[] {
                    new { rule.Control,
                        PatternMatch = rule.Pattern,
                        rule.ValidatorName,
                        AdditionalData = rule.AdditionalData ?? []
                    }
                }
            })
        };
        return Write(Path.Combine(RulesDirectory, fileName), JsonSerializer.Serialize(document));
    }

    public string WriteRulesText(string fileName, string content) {
        return Write(Path.Combine(RulesDirectory, fileName), content);
    }

    public void Dispose() {
        Directory.Delete(Root, recursive: true);
    }

    private static string Write(string path, string content) {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }
}
