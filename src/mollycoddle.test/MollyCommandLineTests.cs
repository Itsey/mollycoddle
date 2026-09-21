namespace mollycoddle.test;

using System;
using System.IO;
using Shouldly;
using Xunit;

public class MollyCommandLineTests {
    [Fact]
    public void GetOptions_ExpandsEnvironmentVariablesAndVersionMarkers() {
        string tempRoot = Path.Combine(Path.GetTempPath(), $"mc-mdl-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        string targetDir = Path.Combine(tempRoot, "target");
        Directory.CreateDirectory(targetDir);

        string envKey = "MOLLY_TEST_RULES_PATH";
        string envVal = Path.Combine(tempRoot, "rules");
        Directory.CreateDirectory(envVal);
        string previous = Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
        Environment.SetEnvironmentVariable(envKey, envVal);
        try {
            var sut = new MollyCommandLine {
                RulesetVersion = "v123",
                RulesFile = $@"%{envKey}%\XXVERSIONNAMEXX.molly",
                PrimaryPath = @"%TEMP%\XXVERSIONNAMEXX\common",
                DirectoryToTarget = targetDir + "\\",
                Debug = "off"
            };

            var result = sut.GetOptions();

            result.RulesFile.ShouldBe(Path.Combine(envVal, "v123.molly"));
            result.PrimaryFilePath.ShouldNotBeNull();
            result.PrimaryFilePath.ShouldContain("v123");
            result.PrimaryFilePath.ShouldNotContain("XXVERSIONNAMEXX");
            result.DirectoryToTarget.ShouldBe(targetDir);
            result.EnableDebug.ShouldBeFalse();
            result.DebugSetting.ShouldBe(string.Empty);
        } finally {
            Environment.SetEnvironmentVariable(envKey, previous);
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public void GetOptions_UsesTempPathWhenNotProvided() {
        string targetDir = Path.GetTempPath().TrimEnd('\\');
        var sut = new MollyCommandLine {
            DirectoryToTarget = targetDir,
            RulesFile = @"C:\dummy.molly",
            Debug = "none"
        };

        var result = sut.GetOptions();

        result.TempPath.ShouldBe(Path.GetTempPath());
        result.EnableDebug.ShouldBeFalse();
    }
}
