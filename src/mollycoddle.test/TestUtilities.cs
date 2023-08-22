namespace mollycoddle.test;

using System.IO;
using Plisky.Test;

internal class TestUtilities {
    private UnitTestHelper u = new UnitTestHelper();

    public void CloseAllFiles() {
        u.ClearUpTestFiles();
    }

    public string GetTestDataFile(TestResourcesReferences contentToLoad) {
        string? testResource = TestResources.GetIdentifiers(contentToLoad);
        if (string.IsNullOrEmpty(testResource)) {
            throw new InvalidDataException("The test data must be populated before the tests can proceed");
        }
        string js = u.GetTestDataFile(testResource, "mollycoddle.testdata", "*.molly") ?? string.Empty;
        return js;
    }

    public string GetTestDataFileContent(TestResourcesReferences contentToLoad) {
        string? testResource = TestResources.GetIdentifiers(contentToLoad);
        if (string.IsNullOrEmpty(testResource)) {
            throw new InvalidDataException("The test data must be populated before the tests can proceed");
        }
        string js = u.GetTestDataFile(testResource, "mollycoddle.testdata", "*.molly") ?? string.Empty;
        return File.ReadAllText(js);
    }
}