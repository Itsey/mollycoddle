namespace mollycoddle.test;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using Plisky.Diagnostics;
using Plisky.Test;
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
}