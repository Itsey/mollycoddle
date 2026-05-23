namespace mollycoddle.test;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
        u = new UnitTestHelper();
        mo = new MollyOptions();
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
    [InlineData("/asdf", "/asdf/asdf/asdf.mols", "asdf", "asdf.mols")]
    [InlineData("/molly", "/molly/default/defaultrules.mollyset", "default", "defaultrules.mollyset")]
    public void ConvertDownloadPathToLocalPath(string molsbaseMarker, string downloadUrl, string versionMarker, string fileName) {
        var sut = new NexusSupport(mo);
        var ret = sut.GetVersionAndFilenameFromNexusUrl(molsbaseMarker, downloadUrl);
        ret.Item1.ShouldBe(versionMarker);
        ret.Item2.ShouldBe(fileName);
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

        cr.DefectCount.ShouldBe(data.Item3 == true ? 1 : 0);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Nexus_rules_missing_marker_throws() {
        b.Info.Flow();

        var sut = new NexusSupport(mo);

        Assert.Throws<InvalidOperationException>(() => {
            _ = sut.GetUrlToUse("http://nomarker/no/marker", "personify");
        });
    }

    [Theory]
    [ClassData(typeof(NexusPrimaryFilesTestData))]
    public void NexusMasterUrlParser(string parseString, NexusConfig expected) {
        var sut = new NexusSupport(mo);
        var result = sut.GetNexusSettings(parseString);

        Assert.NotNull(result);
        result.Repository.ShouldBe(expected.Repository);
        result.SearchPath.ShouldBe(expected.SearchPath);
    }

    [Theory]
    [ClassData(typeof(NexusRulesetTestData))]
    public void NexusMasterUrlParser2(string primaryUrl, NexusConfig expected) {
        var sut = new NexusSupport(mo);
        var result = sut.GetNexusSettings(primaryUrl);

        Assert.NotNull(result);
        result.BasePathUrl.ShouldBe(expected.BasePathUrl);
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
        result.BasePathUrl.ShouldBe(expected.BasePathUrl);
    }

    [Theory]
    [ClassData(typeof(NexusRulesetTestData))]
    public void NexusUrlParser2(string parseString, NexusConfig expected) {
        var sut = new NexusSupport(mo);
        var result = sut.GetNexusSettings(parseString);

        Assert.NotNull(result);
        result.BasePathUrl.ShouldBe(expected.BasePathUrl);
        result.FilenameUrl.ShouldBe(expected.FilenameUrl);
        result.Username.ShouldBe(expected.Username);
        result.Password.ShouldBe(expected.Password);
        result.Server.ShouldBe(expected.Server);
    }

    [Fact]
    public void Split_into_chunks_simple_works() {
        var sut = new NexusSupport(mo);
        string[] markers = new string[] { "[a", "[b", "[c" };
        var chunks = sut.GetChunks("[aone[btwo[cthree", markers);

        chunks.Count.ShouldBe(markers.Length);
        chunks["[a"].Marker.ShouldBe("[a");
        chunks["[a"].Value.ShouldBe("one");
        chunks["[b"].Marker.ShouldBe("[b");
        chunks["[b"].Value.ShouldBe("two");
        chunks["[c"].Marker.ShouldBe("[c");
        chunks["[c"].Value.ShouldBe("three");
    }

    [Fact]
    public void Split_into_chunks_simple_works2() {
        var sut = new NexusSupport(mo);
        string[] markers = new string[] { "[U::", "[P::", "[R::", "[G::", "[L::" };
        var chunks = sut.GetChunks("[U::a[P::b[R::plisky[G::/primaryfiles/default[L::http://somenexusserver/repository/", markers);

        chunks.Count.ShouldBe(markers.Length);
        chunks["[U::"].Marker.ShouldBe("[U::");
        chunks["[U::"].Value.ShouldBe("a");
        chunks["[P::"].Marker.ShouldBe("[P::");
        chunks["[P::"].Value.ShouldBe("b");
    }

    [Fact]
    public void Split_into_chunks_with_null_works() {
        var sut = new NexusSupport(mo);
        var chunks = sut.GetChunks("[aone[btwo[cthree", new string[] { "[a", "[x", "[c" });

        chunks.Count.ShouldBe(3);
        chunks["[a"].Marker.ShouldBe("[a");
        chunks["[a"].Value.ShouldBe("one[btwo");
        chunks["[c"].Marker.ShouldBe("[c");
        chunks["[c"].Value.ShouldBe("three");
        chunks["[x"].Marker.ShouldBe("[x");
        chunks["[x"].Value.ShouldBeNull();
    }

    [Theory]
    [Fresh]
    [MemberData(nameof(StartStopRegex_DataMethod))]
    public void StartStopRegex_Works_AsExpected(StartStopTestData ssd) {
        var sut = new RegexLineValidator(dummyRuleName);
        sut.StartString = ssd.StartString;
        sut.EndString = ssd.StopString;

        foreach (var l in ssd.TestList) {
            sut.IsExecuting(l.Item1).ShouldBe(l.Item2, $"{l.Item1}");
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

        mm.IsMatch(filename).ShouldBe(matchExpected);
    }
}