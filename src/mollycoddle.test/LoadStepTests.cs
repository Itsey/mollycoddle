namespace mollycoddle.test {

    using System;
    using System.Linq;
    using Plisky.Diagnostics;
    using Plisky.Test;
    using Xunit;

    public class LoadStepTests {
        private Bilge b = new Bilge();
        private string TestRuleName = "TestRuleName";
        private UnitTestHelper u = new UnitTestHelper();

        [Fact]
        [Integration]
        public void LoadStep_BadValidator_Throws() {
            b.Info.Flow();

            string jsonvalidatorstep = "{\"ValidatorName\":\"abadname\",\"PatternMatch\":\"a\",\"Control\":\"mustexist\",\"AdditionalData\":null }";
            var sut = new MollyRuleFactory();

            Assert.Throws<InvalidOperationException>(() => {
                var rl = (NugetValidationChecks)sut.LoadValidatorStep(TestRuleName, jsonvalidatorstep);
            });
        }

        [Fact]
        [Integration]
        public void LoadStep_DirectoryValidator_CorrectlyLoads() {
            b.Info.Flow();

            string jsonvalidatorstep = "{\"ValidatorName\":\"DirectoryValidationChecks\",\"PatternMatch\":\"%ROOT%\\\\src\",\"Control\":\"MustExist\",\"AdditionalData\":null}";
            var rule = new MollyRuleFactory();
            var rl = rule.LoadValidatorStep(TestRuleName, jsonvalidatorstep);

            Assert.NotNull(rl);
            Assert.True(rl is DirectoryValidationChecks);
        }

        [Fact]
        [Integration]
        public void LoadStep_Dv_MustExist_CorrectlyLoads() {
            b.Info.Flow();

            string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
            string jsonvalidatorstep = "{\"ValidatorName\":\"DirectoryValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MustExist\",\"AdditionalData\":null}";
            var rule = new MollyRuleFactory();
            var rl = (DirectoryValidationChecks)rule.LoadValidatorStep(TestRuleName, jsonvalidatorstep);

            Assert.NotNull(rl.MustExistExactly().First(x => x == pattern));
        }

        [Fact]
        [Integration]
        public void LoadStep_Dv_MustNotExist_CorrectlyLoads() {
            b.Info.Flow();

            string pattern = "arflebarflegloop";
            string jsonvalidatorstep = "{\"ValidatorName\":\"DirectoryValidationChecks\",\"PatternMatch\":\"" + pattern + "\",\"Control\":\"MustNotExist\",\"AdditionalData\":null}";
            var rule = new MollyRuleFactory();
            var rl = (DirectoryValidationChecks)rule.LoadValidatorStep(TestRuleName, jsonvalidatorstep);

            Assert.NotNull(rl.GetProhibitedPaths().First(x => x.PrimaryPattern == pattern));
        }

        [Fact]
        [Integration]
        public void LoadStep_Dv_ProhibitedExcept_CorrectlyLoads() {
            b.Info.Flow();

            string pattern = "arflebarflegloop";
            string additional1 = "bob";
            string additional2 = "boblett";
            string jsonvalidatorstep = "{\"ValidatorName\":\"DirectoryValidationChecks\",\"PatternMatch\":\"" + pattern + "\",\"Control\":\"ProhibitedExcept\",\"AdditionalData\": [ \"" + additional1 + "\",\"" + additional2 + "\"]}";
            var rule = new MollyRuleFactory();
            var rl = (DirectoryValidationChecks)rule.LoadValidatorStep(TestRuleName, jsonvalidatorstep);

            Assert.True(rl.GetProhibitedPaths().First().PrimaryPattern == pattern);
            Assert.Contains(additional1, rl.GetProhibitedPaths().First().SecondaryList);
            Assert.Contains(additional2, rl.GetProhibitedPaths().First().SecondaryList);
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
            var rl = (FileValidationChecks)rule.LoadValidatorStep(TestRuleName, jsonvalidatorstep);

            Assert.Contains(additional1, rl.FilesThatMustNotExist().First().SecondaryList);
            Assert.Contains(additional2, rl.FilesThatMustNotExist().First().SecondaryList);
        }

        [Fact]
        [Integration]
        public void LoadStep_FileCheck_MustNotExist_Works() {
            b.Info.Flow();

            string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
            string jsonvalidatorstep = "{\"ValidatorName\":\"FileValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MustNotExist\",\"AdditionalData\": null}";
            var rule = new MollyRuleFactory();
            var rl = (FileValidationChecks)rule.LoadValidatorStep(TestRuleName, jsonvalidatorstep);

            Assert.Equal(rl.FilesThatMustNotExist().First().PrimaryPattern, pattern);
        }

        [Fact]
        [Integration]
        public void LoadStep_FileChecks_MasterMatch_Loads() {
            b.Info.Flow();

            string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
            string jsonvalidatorstep = "{\"ValidatorName\":\"FileValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MatchWithMaster\",\"AdditionalData\":[\"bob\"]}";
            var rule = new MollyRuleFactory();
            var rl = (FileValidationChecks)rule.LoadValidatorStep(TestRuleName, jsonvalidatorstep);

            Assert.NotNull(rl.FilesThatMustMatchTheirMaster().First(x => x.PatternForSourceFile == pattern));
            Assert.NotNull(rl.FilesThatMustMatchTheirMaster().First(x => x.FullPathForMasterFile == "bob"));
        }

        [Fact]
        [Integration]
        public void LoadStep_FileChecks_MasterMatch_RequiresAdditionalData() {
            b.Info.Flow();

            string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
            string jsonvalidatorstep = "{\"ValidatorName\":\"FileValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MatchWithMaster\",\"AdditionalData\":null}";
            var rule = new MollyRuleFactory();

            Assert.Throws<InvalidOperationException>(() => {
                _ = (FileValidationChecks)rule.LoadValidatorStep(TestRuleName, jsonvalidatorstep);
            });
        }

        [Fact]
        [Integration]
        public void LoadStep_FileChecks_MustExist_Works() {
            b.Info.Flow();

            string pattern = @"c:\\ar\\%TEST%fle\\barflegloop";
            string jsonvalidatorstep = "{\"ValidatorName\":\"FileValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"MustExist\",\"AdditionalData\":[\"bob\"]}";
            var rule = new MollyRuleFactory();
            var rl = (FileValidationChecks)rule.LoadValidatorStep(TestRuleName, jsonvalidatorstep);

            Assert.Equal(rl.FilesThatMustExist().First(), pattern);
        }

        [Fact]
        [Integration]
        public void LoadStep_NugetBadControl_Throws() {
            b.Info.Flow();

            string jsonvalidatorstep = "{\"ValidatorName\":\"NugetValidationChecks\",\"PatternMatch\":\"a\",\"Control\":\"arfle\",\"AdditionalData\":null }";
            var sut = new MollyRuleFactory();

            Assert.Throws<InvalidOperationException>(() => {
                var rl = (NugetValidationChecks)sut.LoadValidatorStep(TestRuleName, jsonvalidatorstep);
            });
        }

        [Fact]
        [Integration]
        public void LoadStep_NugetBanned_Loads() {
            b.Info.Flow();

            string pattern = @"pattern";
            string bannedEntry1 = "banned1";
            string bannedEntry2 = "banned2";
            string jsonvalidatorstep = "{\"ValidatorName\":\"NugetValidationChecks\",\"PatternMatch\":\"" + pattern.Replace("\\", "\\\\") + "\",\"Control\":\"ProhibitedPackagesList\",\"AdditionalData\":[\"" + bannedEntry1 + "\",\"" + bannedEntry2 + "\"]}";
            var sut = new MollyRuleFactory();

            var rl = (NugetValidationChecks)sut.LoadValidatorStep(TestRuleName, jsonvalidatorstep);

            var l = rl.GetProhibitedPackagesLists().First();
            Assert.NotNull(l.ProhibitedPackages.First(p => p == bannedEntry1));
            Assert.NotNull(l.ProhibitedPackages.First(p => p == bannedEntry2));
        }
    }
}