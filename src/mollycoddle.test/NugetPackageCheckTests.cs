namespace mollycoddle.test {

    using System.IO;
    using System.Linq;
    using Plisky.Diagnostics;
    using Plisky.Test;
    using Xunit;

    public class NugetPackageCheckTests {
        private readonly Bilge b = new();
        private readonly UnitTestHelper u = new();

        [Theory(DisplayName = nameof(NugetVersion_HasBannedVersion))]
        // Test data file has xunit 2.4.1
        [InlineData("xunit", 1)]
        [InlineData("xunit[2.4.1]", 1)]
        [InlineData("xunit[2.3.1]", 0)]
        [InlineData("xunit[0.0.0-2.4.0]", 0)]
        [InlineData("xunit[0.0.0-2.4.2]", 1)]
        [InlineData("xunit[>0.0.0]", 1)]
        [InlineData("xunit[>2.4.0]", 1)]
        [InlineData("xunit[>2.4.1]", 0)]
        [InlineData("xunit[<3.0.0]", 1)]
        [InlineData("xunit[<2.4.1]", 0)]
        [InlineData("xunit[<2.4.2]", 1)]
        [InlineData("xunit[<2.4.0]", 0)]
        public void NugetVersion_HasBannedVersion(string bannedString, int expectedDefectCount) {
            b.Info.Flow();
            
            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            _ = mps.WithRootedFolder("src");

            string? testResource = TestResources.GetIdentifiers(TestResourcesReferences.CsTestProjectWithXunit);
            if (string.IsNullOrEmpty(testResource)) {
                throw new InvalidDataException("The test data must be populated before the tests can proceed");
            }            
            mps.WithRootedFile("bob.test.csproj", File.ReadAllText(testResource));
            var dv = new NugetPackageValidator(MockProjectStructure.DUMMYRULENAME);


            dv.AddProhibitedPackageList(@"**\*.test.csproj", bannedString);

            var sut = new NugetPackageStructureChecker(mps, new MollyOptions());
            sut.AddRuleRequirement(dv);

            var cr = sut.Check();
            Assert.Fail();
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
            string? testResource = TestResources.GetIdentifiers(TestResourcesReferences.CsTestProjectWithXunit);
            if (string.IsNullOrEmpty(testResource)) {
                throw new InvalidDataException("The test data must be populated before the tests can proceed");
            }
            string s = u.GetTestDataFile(testResource);
            mps.WithRootedFile("bob.test.csproj", File.ReadAllText(s));
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

            string? testResource = TestResources.GetIdentifiers(TestResourcesReferences.CsTestProjectWithoutXunit);
            if (string.IsNullOrEmpty(testResource)) {
                throw new InvalidDataException("The test data must be populated before the tests can proceed");
            }

            mps.WithRootedFile("bob.test.csproj", File.ReadAllText(testResource));
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

            string? testResource = TestResources.GetIdentifiers(TestResourcesReferences.CsProjBandPackage);
            if (string.IsNullOrEmpty(testResource)) {
                throw new InvalidDataException("The test data must be populated before the tests can proceed");
            }
            mps.WithRootedFile("bob.csproj", File.ReadAllText(testResource));
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

            string? testResource = TestResources.GetIdentifiers(TestResourcesReferences.CsProjSimpleNugetReferences);
            if (string.IsNullOrEmpty(testResource)) {
                throw new InvalidDataException("The test data must be populated before the tests can proceed");
            }
            string contents = File.ReadAllText(testResource);

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

            string? testResource = TestResources.GetIdentifiers(TestResourcesReferences.CsProjSimpleNugetReferences);
            if (string.IsNullOrEmpty(testResource)) {
                throw new InvalidDataException("The test data must be populated before the tests can proceed");
            }
            string contents = File.ReadAllText(testResource);

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

            string? testResource = TestResources.GetIdentifiers(TestResourcesReferences.CsProjSimpleNugetReferences);
            if (string.IsNullOrEmpty(testResource)) {
                throw new InvalidDataException("The test data must be populated before the tests can proceed");
            }
            string contents = File.ReadAllText(testResource);
            
            var ngps = npc.ReadNugetPackageFromSDKProjectContents(contents);

            Assert.NotEmpty(ngps);
            Assert.NotNull(ngps.First(x => x.RawVersion == "3.1.5"));
            Assert.NotNull(ngps.First(x => x.RawVersion == "2.0.0"));
        }
    }
}