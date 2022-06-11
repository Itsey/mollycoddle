using System.Runtime.CompilerServices;

namespace mollycoddle.test {

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
        MollyRule_EditorConfigMaster,
        MollyRule_GitIgnoreMaster,
        MollyRule_NugetConfigMaster,
        MollyRule_NoNaughtyNugets,
        MollyRule_thesolution,
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
                case TestResourcesReferences.MollyRule_EditorConfigMaster: return "alledconfig.molly";
                case TestResourcesReferences.MollyRule_GitIgnoreMaster: return "allgitignore.molly";
                case TestResourcesReferences.MollyRule_NugetConfigMaster: return "allnugetconfig.molly";
                case TestResourcesReferences.MollyRule_NoNaughtyNugets: return "nonaughtynugets.molly";
                case TestResourcesReferences.MollyRule_thesolution: return "thesolution.molly";
            }
            return null;
        }
    }
}