namespace mollycoddle.test;

using System;
using System.Collections.Generic;
using System.IO;
using Plisky.Test;
using Shouldly;
using Xunit;

public class NexusCacheIndexTests : IDisposable {
    private readonly UnitTestHelper u = new UnitTestHelper();
    private readonly MollyOptions mo = new MollyOptions();
    private readonly string testDir;

    public NexusCacheIndexTests() {
        testDir = Path.Combine(Path.GetTempPath(), "mccache_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testDir);
    }

    public void Dispose() {
        if (Directory.Exists(testDir)) {
            try {
                Directory.Delete(testDir, true);
            } catch {
                // suppress cleanup error in tests
            }
        }
        u.ClearUpTestFiles();
    }

    [Fact]
    [Trait(Traits.Style, Traits.Unit)]
    public void LoadIndex_WhenNoIndexExists_ReturnsEmptyDictionary() {
        var sut = new NexusSupport(mo);
        var index = sut.LoadIndex(testDir);

        index.ShouldNotBeNull();
        index.Count.ShouldBe(0);
    }

    [Fact]
    [Trait(Traits.Style, Traits.Unit)]
    public void SaveIndex_And_LoadIndex_RoundTripsCorrectly() {
        var sut = new NexusSupport(mo);
        var expected = new Dictionary<string, string> {
            ["common.gitignore"] = "9ef10c7376907bf358adf73205a7e495f86964e6",
            ["common.editorconfig"] = "74fd31c1c8bc1fefdc212838654a98d1d2f9a909",
            ["defaultrules.mollyset"] = "c11379aa3eb79e900bc533ad76616acb0e24f4e6"
        };

        sut.SaveIndex(testDir, expected);

        string indexPath = Path.Combine(testDir, NexusSupport.INDEX_FILENAME);
        File.Exists(indexPath).ShouldBeTrue();

        var loaded = sut.LoadIndex(testDir);
        loaded.Count.ShouldBe(expected.Count);
        foreach (var kvp in expected) {
            loaded.ContainsKey(kvp.Key).ShouldBeTrue();
            loaded[kvp.Key].ShouldBe(kvp.Value);
        }
    }

    [Fact]
    [Trait(Traits.Style, Traits.Unit)]
    public void LoadIndex_Reads_AlternativeIndexFilename_WhenIndexJsonMissing() {
        var sut = new NexusSupport(mo);
        string altPath = Path.Combine(testDir, NexusSupport.ALT_INDEX_FILENAME);
        string json = """
        {
            "common.nuget.config": "5f6ef02e3fa90278c5f57b99e40c87679c4e1757"
        }
        """;
        File.WriteAllText(altPath, json);

        var loaded = sut.LoadIndex(testDir);
        loaded.Count.ShouldBe(1);
        loaded["common.nuget.config"].ShouldBe("5f6ef02e3fa90278c5f57b99e40c87679c4e1757");
    }

    [Fact]
    [Trait(Traits.Style, Traits.Unit)]
    public void LoadIndex_Reads_FilesPropertyWrapper() {
        var sut = new NexusSupport(mo);
        string indexPath = Path.Combine(testDir, NexusSupport.INDEX_FILENAME);
        string json = """
        {
            "files": {
                "rule1.molly": "aabbcc112233"
            }
        }
        """;
        File.WriteAllText(indexPath, json);

        var loaded = sut.LoadIndex(testDir);
        loaded.Count.ShouldBe(1);
        loaded["rule1.molly"].ShouldBe("aabbcc112233");
    }

    [Fact]
    [Trait(Traits.Style, Traits.Unit)]
    public void SaveIndex_NormalizesKeys_WithForwardSlashes() {
        var sut = new NexusSupport(mo);
        var data = new Dictionary<string, string> {
            [@"subfolder\rule.molly"] = "1234567890abcdef"
        };

        sut.SaveIndex(testDir, data);

        var loaded = sut.LoadIndex(testDir);
        loaded.ContainsKey("subfolder/rule.molly").ShouldBeTrue();
        loaded["subfolder/rule.molly"].ShouldBe("1234567890abcdef");
    }

    [Fact]
    [Trait(Traits.Style, Traits.Unit)]
    public void ActualSaver_SavesFile_UnderGroupFolder() {
        var sut = new NexusSupport(mo) {
            BasePathToSave = testDir
        };

        byte[] fileData = [1, 2, 3, 4, 5];
        sut.ActualSaver(fileData, "/molly/default/rules/test.molly", "/molly");

        string expectedFile = Path.Combine(testDir, "default", "rules", "test.molly");
        File.Exists(expectedFile).ShouldBeTrue();
        File.ReadAllBytes(expectedFile).ShouldBe(fileData);
    }
}
