namespace mollycoddle.test;

using System.Runtime.CompilerServices;

public enum TestResourcesReferences {
    CsProjSimpleNugetReferences,
    CsProjBandPackage,
    CsTestProjectWithXunit,
    CsTestProjectWithoutXunit,
    MollyRule_OneLanguage,
    MollyRule_BannedNugets,
    MollyRule_PenBeMighty,
    MollyRule_HotSauce,
    MollyRule_GoodRoots,
    MollyRule_EditorConfigSample,
    MollyRule_GitIgnoreMaster,
    MollyRule_NugetConfigMaster,
    MollyRule_NoNaughtyNugets,
    MollyRule_TheSolution,
    SolutionFile_ReleaseCorrect,
    SolutionFile_ReleaseIncorrect
}

public static class TestResources {

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string? GetIdentifiers(TestResourcesReferences refNo) {
        switch (refNo) {
            case TestResourcesReferences.CsProjSimpleNugetReferences: return "simplecsproj.txt";
            case TestResourcesReferences.CsProjBandPackage: return "bannedcsproj.txt";
            case TestResourcesReferences.CsTestProjectWithXunit: return "testproject_withxunit.txt";
            case TestResourcesReferences.CsTestProjectWithoutXunit: return "testproject_withoutxunit.txt";
            case TestResourcesReferences.MollyRule_PenBeMighty: return "penbemighty.molly";
            case TestResourcesReferences.MollyRule_HotSauce: return "hotsauce.molly";
            case TestResourcesReferences.MollyRule_GoodRoots: return "goodroots.molly";
            case TestResourcesReferences.MollyRule_OneLanguage: return "onelanguage.molly";
            case TestResourcesReferences.MollyRule_EditorConfigSample: return "alledconfig.molly";
            case TestResourcesReferences.MollyRule_GitIgnoreMaster: return "allgitignore.molly";
            case TestResourcesReferences.MollyRule_NugetConfigMaster: return "allnugetconfig.molly";
            case TestResourcesReferences.MollyRule_NoNaughtyNugets: return "nonaughtynugets.molly";
            case TestResourcesReferences.MollyRule_TheSolution: return "thesolution.molly";
            case TestResourcesReferences.SolutionFile_ReleaseCorrect: return "correct_release_solution.testdata";
            case TestResourcesReferences.SolutionFile_ReleaseIncorrect: return "incorrect_release_solution.testdata";
        }
        return null;
    }
}