namespace mollycoddle.test;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plisky.Diagnostics;
using Shouldly;
using Xunit;

public class MollyMainTests {
    private readonly Bilge b = new();

    [Fact]
    public async Task DoMollly_ThrowsWhenDirectoryDoesNotExist() {
        var options = new MollyOptions {
            DirectoryToTarget = Path.Combine(Path.GetTempPath(), $"mc-mm-{Guid.NewGuid():N}"),
            RulesFile = "any-file.molly",
            TempPath = Path.GetTempPath()
        };
        var sut = new MollyMain(options, b);

        await Should.ThrowAsync<DirectoryNotFoundException>(async () => await sut.DoMollly());
    }

    [Fact]
    public async Task DoMollly_ThrowsWhenRulesFileMissing() {
        string root = Path.Combine(Path.GetTempPath(), $"mc-mm-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try {
            var options = new MollyOptions {
                DirectoryToTarget = root,
                RulesFile = string.Empty,
                TempPath = Path.GetTempPath()
            };
            var sut = new MollyMain(options, b);

            await Should.ThrowAsync<FileNotFoundException>(async () => await sut.DoMollly());
        } finally {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task DoMollly_ThrowsWhenRulesFileTypeInvalidAndWritesErrorOutput() {
        string root = Path.Combine(Path.GetTempPath(), $"mc-mm-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try {
            string invalidRulesFile = Path.Combine(root, "rules.invalid");
            File.WriteAllText(invalidRulesFile, "this content is ignored because extension is invalid");
            var options = new MollyOptions {
                DirectoryToTarget = root,
                RulesFile = invalidRulesFile,
                TempPath = Path.GetTempPath()
            };
            var outputs = new List<(string Message, OutputType Type)>();
            var sut = new MollyMain(options, b) {
                WriteOutput = (m, t) => outputs.Add((m, t))
            };

            await Should.ThrowAsync<InvalidOperationException>(async () => await sut.DoMollly());
            outputs.ShouldContain(p => p.Type == OutputType.Error && p.Message.Contains("Unable To Read RulesFiles"));
            outputs.ShouldContain(p => p.Type == OutputType.Error && p.Message.Contains("RulesFiles::Error:"));
        } finally {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task DoMollly_WithHelpText_OutputsRuleHelpOnceAndViolationsForEachFailure() {
        string root = Path.Combine(Path.GetTempPath(), $"mc-mm-{Guid.NewGuid():N}");
        string tempPath = Path.Combine(root, "temp");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(tempPath);
        Directory.CreateDirectory(Path.Combine(root, "src"));
        Directory.CreateDirectory(Path.Combine(root, "bannana"));
        Directory.CreateDirectory(Path.Combine(root, "arfle"));

        try {
            var tu = new TestUtilities();
            var options = new MollyOptions {
                DirectoryToTarget = root,
                RulesFile = tu.GetTestDataFile(TestResourcesReferences.MollyRule_GoodRoots),
                TempPath = tempPath,
                AddHelpText = true
            };
            var outputs = new List<(string Message, OutputType Type)>();
            var sut = new MollyMain(options, b) {
                WriteOutput = (m, t) => outputs.Add((m, t))
            };

            var result = await sut.DoMollly();

            result.DefectCount.ShouldBe(3);
            outputs.Count(p => p.Type == OutputType.Info && p.Message.Contains("Further help:")).ShouldBe(1);
            outputs.Count(p => p.Type == OutputType.Violation).ShouldBe(3);
        } finally {
            Directory.Delete(root, true);
        }
    }
}
