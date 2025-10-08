namespace mollycoddle.test;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Minimatch;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;
using Xunit;

public class Exploratory {
    private Bilge b = new Bilge();
    private string dummyRuleName = "drn";
    private MollyOptions mo;
    private UnitTestHelper u;

    public Exploratory() {
        b.Info.Flow();
        u = new Plisky.Test.UnitTestHelper();
        mo = new MollyOptions();
    }

    [Fact(DisplayName = nameof(Nexus_rules_missing_marker_throws))]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Nexus_rules_missing_marker_throws() {
        b.Info.Flow();

        var sut = new NexusSupport(mo);

        Assert.Throws<InvalidOperationException>(() => {
            _ = sut.GetUrlToUse("http://nomarker/no/marker", "personify");
        });
    }

    public static IEnumerable<object[]> MustNotMatchDataMethod() {
        var resultData = new List<object[]>();

        var mnmTestData = new List<Tuple<string, string, bool>>() {
            new Tuple<string, string, bool>($"blinky{Environment.NewLine}blonky{Environment.NewLine}blank","blonky",true)
        };

        resultData.Add(mnmTestData.ToArray());

        return resultData;
    }

    public static IEnumerable<object[]> StartStopRegex_DataMethod() {
        var startTestNullEndTest = new List<StartStopTestData>();

        var startTestNullEnd = new List<Tuple<string, bool>> {
            new Tuple<string, bool>("monkey", false),
            new Tuple<string, bool>("fish", false),
            new Tuple<string, bool>("bananna", false),
            new Tuple<string, bool>("cricket ball", false),
            new Tuple<string, bool>("monkey", false),
            new Tuple<string, bool>("money", false),
            new Tuple<string, bool>("balarney", true),
            new Tuple<string, bool>("baloney", true),
            new Tuple<string, bool>("bicanti", true)
        };
        startTestNullEndTest.Add(new StartStopTestData() {
            StartString = "money",
            StopString = null,
            TestList = startTestNullEnd
        });

        yield return startTestNullEndTest.ToArray();

        var startEndNullTest = new List<StartStopTestData>();
        var startEndNullTestStrings = new List<Tuple<string, bool>> {
            new Tuple<string, bool>("one", true),
            new Tuple<string, bool>("two", true),
            new Tuple<string, bool>("three", true),
            new Tuple<string, bool>("four", true),
            new Tuple<string, bool>("five", true),
            new Tuple<string, bool>("once", true),
            new Tuple<string, bool>("i", true),
            new Tuple<string, bool>("", true)
        };
        startEndNullTest.Add(new StartStopTestData() {
            StartString = null,
            StopString = null,
            TestList = startEndNullTestStrings
        });

        yield return startEndNullTest.ToArray();

        var startNullTest = new List<StartStopTestData>();

        var startNullTestStrings = new List<Tuple<string, bool>> {
            new Tuple<string, bool>("one", true),
            new Tuple<string, bool>("two", true),
            new Tuple<string, bool>("three", true),
            new Tuple<string, bool>("four", false),
            new Tuple<string, bool>("five", false),
            new Tuple<string, bool>("once", false),
            new Tuple<string, bool>("i", false),
            new Tuple<string, bool>("", false)
        };

        startNullTest.Add(new StartStopTestData() {
            StartString = null,
            StopString = "four",
            TestList = startNullTestStrings
        });

        yield return startNullTest.ToArray();
    }

    [Theory]
    [Fresh]
    [MemberData(nameof(MustNotMatchDataMethod))]
    public void MustNotMatch_CausesViolaton_OnMatch(Tuple<string, string, bool> data) {
        b.Info.Flow();

        string root = @"C:\MadeUpFolder";
        var mps = MockProjectStructure.Get().WithRoot(root);
        mps.WithRootedFolder("src");
        mps.WithRootedFile("src\\filename.sln", data.Item1); // $"blinky{Environment.NewLine}blonky{Environment.NewLine}blank");

        var sut = new RegexStructureChecker(mps, mo);
        sut.AddRuleRequirement(new RegexLineValidator(dummyRuleName) {
            MatchType = RegexBehaviour.MustNotMatch,
            FileMinmatch = new Minimatch.Minimatcher("**\\filename.sln"),
            RegexMatch = new Regex(data.Item2) //"blonky");
        });

        var cr = sut.Check();

        cr.DefectCount.Should().Be(data.Item3 == true ? 1 : 0);
    }

    [Theory]
    [Fresh]
    [MemberData(nameof(StartStopRegex_DataMethod))]
    public void StartStopRegex_Works_AsExpected(StartStopTestData ssd) {
        var sut = new RegexLineValidator(dummyRuleName);
        sut.StartString = ssd.StartString;
        sut.EndString = ssd.StopString;

        foreach (var l in ssd.TestList) {
            sut.IsExecuting(l.Item1).Should().Be(l.Item2, $"{l.Item1}");
        }
    }

    [Theory]
    [InlineData(".\\mollycoddle\\src\\mollycoddle.sln", "**\\*.sln", true)]
    [InlineData(".\\mollycoddle\\src\\mollycoddle.sln", "**\\src\\*.sln", true)]
    [InlineData("c:\\mollycoddle\\src\\mollycoddle.sln", "**\\src\\*.sln", true)]
    public void TestMinmatchExpressions(string filename, string pattern, bool matchExpected) {
        // This is required to correctly resolve paths that are not rooted, leaving this test in to keep this reminder.
        filename = Path.GetFullPath(filename);
        var mm = new Minimatcher(pattern, new Options() { AllowWindowsPaths = true, IgnoreCase = true });

        mm.IsMatch(filename).Should().Be(matchExpected);
    }

    [Theory]
    [ClassData(typeof(NexusRulesetTestData))]
    public void NexusUrlParser2(string parseString, NexusConfig expected) {
        var sut = new NexusSupport(mo);
        var result = sut.GetNexusSettings(parseString);

        Assert.NotNull(result);
        result.BasePathUrl.Should().Be(expected.BasePathUrl);
        result.FilenameUrl.Should().Be(expected.FilenameUrl);
        result.Username.Should().Be(expected.Username);
        result.Password.Should().Be(expected.Password);
        result.Server.Should().Be(expected.Server);
    }

    [Theory]
    [ClassData(typeof(NexusPrimaryFilesTestData))]
    public void NexusMasterUrlParser(string parseString, NexusConfig expected) {
        var sut = new NexusSupport(mo);
        var result = sut.GetNexusSettings(parseString);

        Assert.NotNull(result);
        //result.BasePathUrl.Should().Be(expected.BasePathUrl);
        result.Repository.Should().Be(expected.Repository);
        result.SearchPath.Should().Be(expected.SearchPath);
    }

    [Fact]
    public void Split_into_chunks_simple_works2() {
        var sut = new NexusSupport(mo);
        string[] markers = new string[] { "[U::", "[P::", "[R::", "[G::", "[L::" };
        var chunks = sut.GetChunks("[U::a[P::b[R::plisky[G::/primaryfiles/default[L::http://somenexusserver/repository/", markers);

        chunks.Count.Should().Be(markers.Length);
        chunks["[U::"].Marker.Should().Be("[U::");
        chunks["[U::"].Value.Should().Be("a");
        chunks["[P::"].Marker.Should().Be("[P::");
        chunks["[P::"].Value.Should().Be("b");
    }

    [Fact]
    public void Split_into_chunks_simple_works() {
        var sut = new NexusSupport(mo);
        string[] markers = new string[] { "[a", "[b", "[c" };
        var chunks = sut.GetChunks("[aone[btwo[cthree", markers);

        chunks.Count.Should().Be(markers.Length);
        chunks["[a"].Marker.Should().Be("[a");
        chunks["[a"].Value.Should().Be("one");
        chunks["[b"].Marker.Should().Be("[b");
        chunks["[b"].Value.Should().Be("two");
        chunks["[c"].Marker.Should().Be("[c");
        chunks["[c"].Value.Should().Be("three");
    }

    [Fact]
    public void Split_into_chunks_with_null_works() {
        var sut = new NexusSupport(mo);
        var chunks = sut.GetChunks("[aone[btwo[cthree", new string[] { "[a", "[x", "[c" });

        chunks.Count.Should().Be(3);
        chunks["[a"].Marker.Should().Be("[a");
        chunks["[a"].Value.Should().Be("one[btwo");
        chunks["[c"].Marker.Should().Be("[c");
        chunks["[c"].Value.Should().Be("three");
        chunks["[x"].Marker.Should().Be("[x");
        chunks["[x"].Value.Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(NexusRulesetTestData))]
    public void NexusMasterUrlParser2(string primaryUrl, NexusConfig expected) {
        var sut = new NexusSupport(mo);
        var result = sut.GetNexusSettings(primaryUrl);

        Assert.NotNull(result);
        result.BasePathUrl.Should().Be(expected.BasePathUrl);
    }
    [Theory]
    [ClassData(typeof(NexusRulesetTestData))]
    public void NexusMasterUrlParserNoTrailingSlash(string primaryUrl, NexusConfig expected) {
        var sut = new NexusSupport(mo);

        // remove trailing slash
        if (primaryUrl.EndsWith('/')) {
            primaryUrl = primaryUrl.Substring(0, primaryUrl.Length - 1);
        }
        var result = sut.GetNexusSettings(primaryUrl);

        Assert.NotNull(result);
        result.BasePathUrl.Should().Be(expected.BasePathUrl);
    }

    [Theory]
    [InlineData("/asdf", "/asdf/asdf/asdf.mols", "asdf", "asdf.mols")]
    [InlineData("/molly", "/molly/default/defaultrules.mollyset", "default", "defaultrules.mollyset")]
    public void ConvertDownloadPathToLocalPath(string molsbaseMarker, string downloadUrl, string versionMarker, string fileName) {
        var sut = new NexusSupport(mo);
        var ret = sut.GetVersionAndFilenameFromNexusUrl(molsbaseMarker, downloadUrl);
        ret.Item1.Should().Be(versionMarker);
        ret.Item2.Should().Be(fileName);
    }

    [Fact]
    public void MustNotExistRule_DoesNotViolate_WhenFileIsIgnoredByGitignore() {
        string root = @"c:\MadeUpPath";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("node_modules");
        mps.WithRootedFile("node_modules\\1.js", "some node stuff");
        mps.WithRootedFile("%ROOT%\\.gitignore", "node_modules/");
        var sut = new MockFileStructureChecker(mps);
        var pattern = new MatchWithSecondaryMatches("**/1.js") {
            SecondaryList = new[] { "%ROOT%\\.gitignore" }
        };

        sut.AssignMustNotExistAction("TestRule", pattern);
        var result = sut.Check();

        result.DefectCount.ShouldBe(0);
    }

    [Fact]
    public void MustNotExistRule_Violates_WhenFileIsNotIgnoredByGitignore() {
        string root = @"c:\MadeUpPath";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("node_modules");
        mps.WithRootedFile("node_modules\\1.js", "some node stuff");
        mps.WithRootedFile("%ROOT%\\.gitignore", "nothing to see here");
        var sut = new MockFileStructureChecker(mps);
        var pattern = new MatchWithSecondaryMatches("**/1.js") {
            SecondaryList = new[] { "%ROOT%\\.gitignore" }
        };

        sut.AssignMustNotExistAction("TestRule", pattern);
        var result = sut.Check();

        result.DefectCount.ShouldBe(1);
    }


    [Fact]
    public void MustNotExistRule_Violates_WhenGitignoreMissingOrEmpty() {
        string root = @"c:\MadeUpPath";
        var mps = MockProjectStructure.Get().WithRoot(root);
        _ = mps.WithRootedFolder("node_modules");
        mps.WithRootedFile("node_modules\\1.js", "some node stuff");
        var sut = new MockFileStructureChecker(mps);
        var pattern = new MatchWithSecondaryMatches("**/1.js") {
            SecondaryList = new[] { "%ROOT%\\.gitignore" }
        };

        sut.AssignMustNotExistAction("TestRule", pattern);
        var result = sut.Check();

        result.DefectCount.ShouldBe(1);
    }
}