namespace mollycoddle.test;

using System;
using System.Linq;
using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

public class LoadStepTests {
    private Bilge b = new Bilge();
    private string testRuleName = "TestRuleName";

    [Fact]
    [Integration]
    public void LoadStep_BadValidator_Throws() {
        b.Info.Flow();

        string jsonValidatorStep = "{\"ValidatorName\":\"abadname\",\"PatternMatch\":\"a\",\"Control\":\"mustexist\",\"AdditionalData\":null }";
        var sut = new MollyRuleFactory();

        Assert.Throws<InvalidOperationException>(() => {
            var rl = (NugetPackageValidator)sut.LoadValidatorStep(testRuleName, jsonValidatorStep);
        });
    }

    [Fact]
    [Build(BuildType.Any)]
    public void LoadStep_DirectoryBypass_Loads() {
        b.Info.Flow();

        string allowAllPattern = "bungleTheFirst"; ;
        string jsonValidatorStep = "{\"ValidatorName\":\"DirectoryValidationChecks\",\"PatternMatch\":\"" + allowAllPattern + "\",\"Control\":\"FullBypass\",\"AdditionalData\":null}";
        var sut = new MollyRuleFactory();

        var rl = (DirectoryValidator)sut.LoadValidatorStep(testRuleName, jsonValidatorStep);

        Assert.NotNull(rl.FullBypasses());
        Assert.NotNull(rl.FullBypasses().First(p => p == allowAllPattern));
    }

    [Fact]
    [Integration]
    public void LoadStep_DirectoryValidator_CorrectlyLoads() {
        b.Info.Flow();

        string jsonValidatorStep = "{\"ValidatorName\":\"DirectoryValidationChecks\",\"PatternMatch\":\"%ROOT%\\\\src\",\"Control\":\"MustExist\",\"AdditionalData\":null}";
        var rule = new MollyRuleFactory();
        var rl = rule.LoadValidatorStep(testRuleName, jsonValidatorStep);

        Assert.NotNull(rl);
        Assert.True(rl is DirectoryValidator);
    }

    [Fact]
    [Integration]
    public void LoadStep_Dv_MustExist_CorrectlyLoads() {
        b.Info.Flow();

        string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
        string jsonValidatorStep = "{\"ValidatorName\":\"DirectoryValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MustExist\",\"AdditionalData\":null}";
        var rule = new MollyRuleFactory();
        var rl = (DirectoryValidator)rule.LoadValidatorStep(testRuleName, jsonValidatorStep);

        Assert.NotNull(rl.MustExistExactly().First(x => x == pattern));
    }

    [Fact]
    [Integration]
    public void LoadStep_Dv_MustNotExist_CorrectlyLoads() {
        b.Info.Flow();

        string pattern = "arflebarflegloop";
        string jsonValidatorStep = "{\"ValidatorName\":\"DirectoryValidationChecks\",\"PatternMatch\":\"" + pattern + "\",\"Control\":\"MustNotExist\",\"AdditionalData\":null}";
        var rule = new MollyRuleFactory();
        var rl = (DirectoryValidator)rule.LoadValidatorStep(testRuleName, jsonValidatorStep);

        Assert.NotNull(rl.GetProhibitedPaths().First(x => x.PrimaryPattern == pattern));
    }

    [Fact]
    [Integration]
    public void LoadStep_Dv_ProhibitedExcept_CorrectlyLoads() {
        b.Info.Flow();

        string pattern = "arflebarflegloop";
        string additional1 = "bob";
        string additional2 = "boblett";
        string jsonValidatorStep = "{\"ValidatorName\":\"DirectoryValidationChecks\",\"PatternMatch\":\"" + pattern + "\",\"Control\":\"ProhibitedExcept\",\"AdditionalData\": [ \"" + additional1 + "\",\"" + additional2 + "\"]}";
        var rule = new MollyRuleFactory();
        var rl = (DirectoryValidator)rule.LoadValidatorStep(testRuleName, jsonValidatorStep);

        Assert.True(rl.GetProhibitedPaths().First().PrimaryPattern == pattern);
        Assert.Contains(additional1, rl.GetProhibitedPaths().First().SecondaryList);
        Assert.Contains(additional2, rl.GetProhibitedPaths().First().SecondaryList);
    }

    [Fact]
    [Build(BuildType.Any)]
    public void LoadStep_FileBypass_Loads() {
        b.Info.Flow();

        string allowAllPattern = "bungleTheFirst";

        string jsonValidatorStep = "{\"ValidatorName\":\"FileValidationChecks\",\"PatternMatch\":\"" + allowAllPattern + "\",\"Control\":\"FullBypass\",\"AdditionalData\":null}";
        var sut = new MollyRuleFactory();

        var rl = (FileValidator)sut.LoadValidatorStep(testRuleName, jsonValidatorStep);

        Assert.NotNull(rl.FullBypasses());
        Assert.NotNull(rl.FullBypasses().First(p => p == allowAllPattern));
    }

    [Fact]
    [Integration]
    public void LoadStep_FileCheck_MustNotExist_HasExceptions() {
        b.Info.Flow();

        string additional1 = "bob";
        string additional2 = "boblett";
        string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
        string jsonvalidatorstep = "{\"ValidatorName\":\"FileValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MustNotExist\",\"AdditionalData\": [ \"" + additional1 + "\",\"" + additional2 + "\"]}";
        var rule = new MollyRuleFactory();
        var rl = (FileValidator)rule.LoadValidatorStep(testRuleName, jsonvalidatorstep);

        Assert.Contains(additional1, rl.FilesThatMustNotExist().First().SecondaryList);
        Assert.Contains(additional2, rl.FilesThatMustNotExist().First().SecondaryList);
    }

    [Fact]
    [Integration]
    public void LoadStep_FileCheck_MustNotExist_Works() {
        b.Info.Flow();

        string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
        string jsonValidatorStep = "{\"ValidatorName\":\"FileValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MustNotExist\",\"AdditionalData\": null}";
        var rule = new MollyRuleFactory();
        var rl = (FileValidator)rule.LoadValidatorStep(testRuleName, jsonValidatorStep);

        Assert.Equal(rl.FilesThatMustNotExist().First().PrimaryPattern, pattern);
    }

    [Fact]
    [Integration]
    public void LoadStep_FileChecks_common_match_loads() {
        b.Info.Flow();

        string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
        string jsonValidatorStep = "{\"ValidatorName\":\"FileValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MatchWithPrimary\",\"AdditionalData\":[\"bob\"]}";
        var rule = new MollyRuleFactory();
        var rl = (FileValidator)rule.LoadValidatorStep(testRuleName, jsonValidatorStep);

        Assert.NotNull(rl.FilesThatMustMatchTheirCommon().First(x => x.PatternForSourceFile == pattern));
        Assert.NotNull(rl.FilesThatMustMatchTheirCommon().First(x => x.FullPathForCommonFile == "bob"));
    }

    [Fact]
    [Integration]
    public void LoadStep_FileChecks_common_match_RequiresAdditionalData() {
        b.Info.Flow();

        string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
        string jsonValidatorStep = "{\"ValidatorName\":\"FileValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MatchWithPrimary\",\"AdditionalData\":null}";
        var rule = new MollyRuleFactory();

        Assert.Throws<InvalidOperationException>(() => {
            _ = (FileValidator)rule.LoadValidatorStep(testRuleName, jsonValidatorStep);
        });
    }

    [Fact]
    [Integration]
    public void LoadStep_FileChecks_MustExist_Works() {
        b.Info.Flow();

        string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
        string jsonValidatorStep = "{\"ValidatorName\":\"FileValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MustExist\",\"AdditionalData\":[\"bob\"]}";
        var rule = new MollyRuleFactory();
        var rl = (FileValidator)rule.LoadValidatorStep(testRuleName, jsonValidatorStep);

        Assert.Equal(rl.FilesThatMustExist().First(), pattern);
    }

    [Fact]
    [Integration]
    public void LoadStep_NugetBadControl_Throws() {
        b.Info.Flow();

        string jsonValidatorStep = "{\"ValidatorName\":\"NugetValidationChecks\",\"PatternMatch\":\"a\",\"Control\":\"arfle\",\"AdditionalData\":null }";
        var sut = new MollyRuleFactory();

        Assert.Throws<InvalidOperationException>(() => {
            var rl = (NugetPackageValidator)sut.LoadValidatorStep(testRuleName, jsonValidatorStep);
        });
    }

    [Fact]
    [Integration]
    public void LoadStep_NugetBanned_Loads() {
        b.Info.Flow();

        string pattern = @"pattern";
        string bannedEntry1 = "banned1";
        string bannedEntry2 = "banned2";
        string jsonValidatorStep = "{\"ValidatorName\":\"NugetValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"ProhibitedPackagesList\",\"AdditionalData\":[\"" + bannedEntry1 + "\",\"" + bannedEntry2 + "\"]}";
        var sut = new MollyRuleFactory();

        var rl = (NugetPackageValidator)sut.LoadValidatorStep(testRuleName, jsonValidatorStep);

        var l = rl.GetProhibitedPackagesLists().First();
        Assert.NotNull(l.ProhibitedPackages.First(p => p.PackageName == bannedEntry1));
        Assert.NotNull(l.ProhibitedPackages.First(p => p.PackageName == bannedEntry2));
    }
}