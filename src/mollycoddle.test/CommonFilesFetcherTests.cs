namespace mollycoddle.test;

using System;
using System.IO;
using Plisky.Diagnostics;
using Shouldly;
using Xunit;

public class CommonFilesFetcherTests {
    private readonly Bilge b = new();

    [Fact]
    public void FetchCommonFiles_ThrowsWhenPrimaryPathMissing() {
        string root = Path.Combine(Path.GetTempPath(), $"mc-cff-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try {
            var options = new MollyOptions {
                DirectoryToTarget = root,
                RulesFile = Path.Combine(root, "rules.molly"),
                PrimaryFilePath = Path.Combine(root, "does-not-exist")
            };
            File.WriteAllText(options.RulesFile, "{}");
            var sut = new CommonFilesFetcher(options, b);

            Should.Throw<Exception>(() => sut.FetchCommonFiles());
        } finally {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void FetchCommonFiles_ReturnsMinusOneWhenNoCommonMappingsInRules() {
        string root = Path.Combine(Path.GetTempPath(), $"mc-cff-{Guid.NewGuid():N}");
        string primary = Path.Combine(root, "primary");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(primary);
        try {
            var tu = new TestUtilities();
            var options = new MollyOptions {
                DirectoryToTarget = root,
                RulesFile = tu.GetTestDataFile(TestResourcesReferences.MollyRule_GoodRoots),
                PrimaryFilePath = primary
            };
            var sut = new CommonFilesFetcher(options, b);

            int result = sut.FetchCommonFiles();

            result.ShouldBe(-1);
        } finally {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void FetchCommonFiles_CopiesCommonFileToExpectedTargetPath() {
        const string CONTENTS = "root=true";
        string root = Path.Combine(Path.GetTempPath(), $"mc-cff-{Guid.NewGuid():N}");
        string primary = Path.Combine(root, "primary");
        string src = Path.Combine(root, "src");
        Directory.CreateDirectory(primary);
        Directory.CreateDirectory(src);
        try {
            string commonSource = Path.Combine(primary, "common.editorconfig");
            File.WriteAllText(commonSource, CONTENTS);

            var tu = new TestUtilities();
            var options = new MollyOptions {
                DirectoryToTarget = root,
                RulesFile = tu.GetTestDataFile(TestResourcesReferences.MollyRule_EditorConfigSample),
                PrimaryFilePath = primary
            };
            var sut = new CommonFilesFetcher(options, b);

            int result = sut.FetchCommonFiles();
            string copiedFile = Path.Combine(src, ".editorconfig");

            result.ShouldBe(0);
            File.Exists(copiedFile).ShouldBeTrue();
            File.ReadAllText(copiedFile).ShouldBe(CONTENTS);
        } finally {
            Directory.Delete(root, true);
        }
    }
}
