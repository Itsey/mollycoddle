namespace mollycoddle.test {

    using System;
    using System.Linq;
    using FluentAssertions;
    using Plisky.Diagnostics;
    using Plisky.Test;
    using Xunit;

    public class NugetPackageCheckTests {
        private readonly Bilge b = new();
        private readonly UnitTestHelper u = new();
        private readonly TestUtilities tu = new();

        public NugetPackageCheckTests() {
            b.Assert.ConfigureAsserts(AssertionStyle.Nothing);
        }

        [Fact(DisplayName = nameof(ParseNugetString_ThrowsIfNull))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ParseNugetString_ThrowsIfNull() {
            b.Info.Flow();

            Assert.Throws<ArgumentNullException>(() => {
#pragma warning disable CS8625
                // Null check disabled in compiler, as ideally they wouldnt pass null but we throw if they do anyhow.
                var nvpi = new NugetPackageValidatorInternals("dummy-rule");
                var sut = nvpi.ParsePackageListStringToPackageReference_Test(null);
#pragma warning restore CS8625
            });
        }

        [Theory(DisplayName = nameof(ParseNugetString_Works))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        [InlineData("packageName", "packageName", null, null, PackageVersionMatchType.AllVersions)]
        [InlineData("packageName[2.4.1]", "packageName", "2.4.1.0", "2.4.1.0", PackageVersionMatchType.Exact)]
        [InlineData("packageName[1.0]", "packageName", "1.0.0.0", "1.0.0.0", PackageVersionMatchType.Exact)]
        [InlineData("packageName[0.0.0.1]", "packageName", "0.0.0.1", "0.0.0.1", PackageVersionMatchType.Exact)]
        [InlineData("packageName[<1.0]", "packageName", "1.0.0.0", "0.0.0.0", PackageVersionMatchType.NotLessThan)]
        [InlineData("packageName[>1.0]", "packageName", "0.0.0.0", "1.0.0.0", PackageVersionMatchType.NotMoreThan)]
        [InlineData("packageName[1.0-2.0]", "packageName", "1.0.0.0", "2.0.0.0", PackageVersionMatchType.RangeProhibited)]
        [InlineData("moq[4.18.4-4.20.69]", "moq", "4.18.4.0", "4.20.69.0", PackageVersionMatchType.RangeProhibited)]
        public void ParseNugetString_Works(string packageString, string packageName, string mustBeAtLeast, string mustBeAtMost, PackageVersionMatchType mt) {
            b.Info.Flow();
            var nvpi = new NugetPackageValidatorInternals("dummy-rule");
            var sut = nvpi.ParsePackageListStringToPackageReference_Test(packageString);

            if (mustBeAtLeast == null) {
                _ = sut.LowVersionNumber.Should().BeNull();
            } else {
                _ = sut.LowVersionNumber.Should().NotBeNull();
                _ = (sut.LowVersionNumber?.ToString().Should().BeEquivalentTo(mustBeAtLeast));
            }
            if (mustBeAtMost == null) {
                _ = sut.LowVersionNumber.Should().BeNull();
            } else {
                _ = sut.HighVersionNumber.Should().NotBeNull();
                _ = (sut.HighVersionNumber?.ToString().Should().BeEquivalentTo(mustBeAtMost));
            }

            _ = sut.PackageName.Should().BeEquivalentTo(packageName);
            _ = sut.VersionMatchType.Should().Be(mt);
        }

        [Theory(DisplayName = nameof(NugetVersion_HasBannedVersion))]
        // Test data file has xunit 2.4.1
        [InlineData("xunit", 1)]
        [InlineData("xunit[2.4.1]", 1)]
        [InlineData("xunit[2.3.1]", 0)]
        [InlineData("xunit[0.0.0-2.4.0]", 0)]   // Must not be between...
        [InlineData("xunit[0.0.0-2.4.2]", 1)]
        [InlineData("xunit[2.0-3.0]", 1)]
        [InlineData("xunit[1.0-2.0]", 0)]
        [InlineData("xunit[>0.0.0]", 1)]
        [InlineData("xunit[>2.4.0]", 1)]  // Can not be greater than
        [InlineData("xunit[>2.4.1]", 0)]
        [InlineData("xunit[<3.0.0]", 1)]  // Can not be less than
        [InlineData("xunit[<2.4.1]", 0)]
        [InlineData("xunit[<2.4.2]", 1)]
        [InlineData("xunit[<2.4.0]", 0)]
        [InlineData("moq[4.18.4-4.20.69]", 0)]
        [InlineData("moq[>4.18.4.0]", 0)]
        public void NugetVersion_HasBannedVersion(string bannedString, int expectedDefectCount) {
            b.Info.Flow();

            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            _ = mps.WithRootedFolder("src");

            string contents = tu.GetTestDataFileContent(TestResourcesReferences.CsTestProjectWithXunit);
            mps.WithRootedFile("bob.test.csproj", contents);
            var dv = new NugetPackageValidator(MockProjectStructure.DUMMYRULENAME);
            dv.AddProhibitedPackageList(@"**\*.test.csproj", bannedString);
            var sut = new NugetPackageStructureChecker(mps, new MollyOptions());
            sut.AddRuleRequirement(dv);

            var cr = sut.Check();

            Assert.Equal(expectedDefectCount, cr.DefectCount);
        }

        [Fact(DisplayName = nameof(NugetMustContainPackage_NoErrorIfFound))]
        [Build(BuildType.Release)]
        [Integration]
        public void NugetMustContainPackage_NoErrorIfFound() {
            b.Info.Flow();

            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            _ = mps.WithRootedFolder("src");

            string contents = tu.GetTestDataFileContent(TestResourcesReferences.CsTestProjectWithXunit);
            mps.WithRootedFile("bob.test.csproj", contents);

            var dv = new NugetPackageValidator(MockProjectStructure.DUMMYRULENAME);
            dv.AddMustReferencePackageList(@"**\*.test.csproj", "xunit");

            var sut = new NugetPackageStructureChecker(mps, new MollyOptions());
            sut.AddRuleRequirement(dv);

            var cr = sut.Check();

            Assert.Equal(0, cr.DefectCount);
        }

        [Fact(DisplayName = nameof(NugetMustContainPackage_ErrorsIfNotFound))]
        [Build(BuildType.Release)]
        [Integration]
        public void NugetMustContainPackage_ErrorsIfNotFound() {
            b.Info.Flow();

            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            _ = mps.WithRootedFolder("src");

            string contents = tu.GetTestDataFileContent(TestResourcesReferences.CsTestProjectWithoutXunit);

            mps.WithRootedFile("bob.test.csproj", contents);
            var dv = new NugetPackageValidator(MockProjectStructure.DUMMYRULENAME);
            dv.AddMustReferencePackageList(@"**\*.test.csproj", "xunit");

            var sut = new NugetPackageStructureChecker(mps, new MollyOptions());
            sut.AddRuleRequirement(dv);

            var cr = sut.Check();

            Assert.Equal(1, cr.DefectCount);
        }

        [Fact]
        [Build(BuildType.Release)]
        [Integration]
        public void BannedPackageRule_ResultsInViolation() {
            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            _ = mps.WithRootedFolder("src");

            string contents = tu.GetTestDataFileContent(TestResourcesReferences.CsProjBandPackage);
            mps.WithRootedFile("bob.csproj", contents);
            var dv = new NugetPackageValidator(MockProjectStructure.DUMMYRULENAME);
            dv.AddProhibitedPackageList(@"**\*.csproj", "banned.package");

            var sut = new NugetPackageStructureChecker(mps, new MollyOptions());
            sut.AddRuleRequirement(dv);

            var cr = sut.Check();

            Assert.Equal(1, cr.DefectCount);
        }

        [Fact]
        [Build(BuildType.Release)]
        [Integration]
        public void NugetPackageLoad_DoesNotFindDuffData() {
            b.Info.Flow();

            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            var npc = new NugetPackageStructureChecker(mps, new MollyOptions());
            string contents = tu.GetTestDataFileContent(TestResourcesReferences.CsProjSimpleNugetReferences);

            var ngps = npc.ReadNugetPackageFromSDKProjectContents(contents);

            Assert.Null(ngps.FirstOrDefault(x => x.PackageIdentifier == "xxx"));
            Assert.Null(ngps.FirstOrDefault(x => x.PackageIdentifier == "nlog"));
        }

        [Fact]
        [Build(BuildType.Release)]
        [Integration]
        public void NugetPackageLoad_FindsPackageNames() {
            b.Info.Flow();

            string root = @"c:\MadeUpPath";
            var mps = MockProjectStructure.Get().WithRoot(root);
            var npc = new NugetPackageStructureChecker(mps, new MollyOptions());

            string contents = tu.GetTestDataFileContent(TestResourcesReferences.CsProjSimpleNugetReferences);

            var ngps = npc.ReadNugetPackageFromSDKProjectContents(contents);

            Assert.NotEmpty(ngps);
            Assert.NotNull(ngps.First(x => x.PackageIdentifier == "Plisky.Diagnostics"));
            Assert.NotNull(ngps.First(x => x.PackageIdentifier == "Plisky.Listeners"));
        }

        [Fact]
        [Build(BuildType.Release)]
        [Integration]
        public void NugetPackageLoad_FindsPackageVersions() {
            b.Info.Flow();

            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            var npc = new NugetPackageStructureChecker(mps, new MollyOptions());

            string contents = tu.GetTestDataFileContent(TestResourcesReferences.CsProjSimpleNugetReferences);
            var ngps = npc.ReadNugetPackageFromSDKProjectContents(contents);

            Assert.NotEmpty(ngps);
            Assert.NotNull(ngps.First(x => x.RawVersion == "3.1.5"));
            Assert.NotNull(ngps.First(x => x.RawVersion == "2.0.0"));
        }
    }
}