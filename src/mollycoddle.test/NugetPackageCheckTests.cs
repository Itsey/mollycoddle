namespace mollycoddle.test {

    using System.IO;
    using System.Linq;
    using Plisky.Diagnostics;
    using Plisky.Test;
    using Xunit;

    public class NugetPackageCheckTests {
        private Bilge b = new Bilge();
        private UnitTestHelper u = new UnitTestHelper();

        [Fact(DisplayName = nameof(NugetMustContainPackage_NoErrorIfFound))]
        [Build(BuildType.Release)]
        [Integration]
        public void NugetMustContainPackage_NoErrorIfFound() {
            b.Info.Flow();

            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            mps.WithRootedFolder("src");
            var s = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.CsTestProjectWithXunit));
            mps.WithRootedFile("bob.test.csproj", File.ReadAllText(s));
            var dv = new NugetValidationChecks(MockProjectStructure.DUMMYRULENAME);
            dv.AddMustReferencePackageList(@"**\*.test.csproj", "xunit");

            var sut = new NugetPackageChecker(mps, new MollyOptions());
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
            mps.WithRootedFolder("src");
            var s = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.CsTestProjectWithoutXunit));
            mps.WithRootedFile("bob.test.csproj", File.ReadAllText(s));
            var dv = new NugetValidationChecks(MockProjectStructure.DUMMYRULENAME);
            dv.AddMustReferencePackageList(@"**\*.test.csproj", "xunit");

            var sut = new NugetPackageChecker(mps, new MollyOptions());
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
            mps.WithRootedFolder("src");
            var s = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.CsProjBandPackage));
            mps.WithRootedFile("bob.csproj", File.ReadAllText(s));
            var dv = new NugetValidationChecks(MockProjectStructure.DUMMYRULENAME);
            dv.AddProhibitedPackageList(@"**\*.csproj", "banned.package");

            var sut = new NugetPackageChecker(mps, new MollyOptions());
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
            var npc = new NugetPackageChecker(mps, new MollyOptions());

            var s = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.CsProjSimpleNugetReferences));
            string contents = File.ReadAllText(s);

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
            var npc = new NugetPackageChecker(mps, new MollyOptions());

            var s = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.CsProjSimpleNugetReferences));
            string contents = File.ReadAllText(s);

            var ngps = npc.ReadNugetPackageFromSDKProjectContents(contents);

            Assert.NotEmpty(ngps);
            Assert.NotNull(ngps.First(x => x.PackageIdentifier == "Plisky.Diagnostics"));
            Assert.NotNull(ngps.First(x => x.PackageIdentifier == "Plisky.Listeners"));
        }

        [Fact]
        [Build(BuildType.Release)][Integration]
        public void NugetPackageLoad_FindsPackageVersions() {
            b.Info.Flow();

            string root = @"C:\MadeUpFolder";
            var mps = MockProjectStructure.Get().WithRoot(root);
            var npc = new NugetPackageChecker(mps, new MollyOptions());

            var s = u.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.CsProjSimpleNugetReferences));
            string contents = File.ReadAllText(s);

            var ngps = npc.ReadNugetPackageFromSDKProjectContents(contents);

            Assert.NotEmpty(ngps);
            Assert.NotNull(ngps.First(x => x.Version == "3.1.5"));
            Assert.NotNull(ngps.First(x => x.Version == "2.0.0"));
        }
    }
}