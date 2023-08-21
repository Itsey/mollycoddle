namespace mollycoddle.test; 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

internal class MockProjectStructure : ProjectStructure {
    public const string DUMMYRULENAME = "testrule";
    public Dictionary<string, string> testFileContents = new();

    public override string? GetFileContents(string filename) {
        return AllFiles.Contains(filename) ? testFileContents[filename] : null;
    }

    public override Tuple<long, byte[]> GetFileHashAndLength(string masterContentsPath) {
        string cnts = testFileContents[masterContentsPath];
        return new Tuple<long, byte[]>(cnts.Length, Encoding.UTF8.GetBytes(cnts));
    }

    internal static MockProjectStructure Get() {
        return new MockProjectStructure();
    }

    internal void WithFile(string fullFileName, string fileContents) {
        AllFiles.Add(fullFileName);
        testFileContents.Add(fullFileName, fileContents);
    }

    internal MockProjectStructure WithFolder(params string[] folders) {
        foreach (string nextPath in folders) {
            base.AllFolders.Add(nextPath);
        }
        return this;
    }

    internal MockProjectStructure WithRoot(string v) {
        base.Root = v;
        return this;
    }

    internal void WithRootedFile(string fileName, string fileContents = "test") {
        fileName = fileName.Contains("%ROOT%") ? fileName.Replace("%ROOT%", base.Root) : Path.Combine(base.Root, fileName);
        AllFiles.Add(fileName);
        testFileContents.Add(fileName, fileContents);
    }

    internal MockProjectStructure WithRootedFolder(params string[] rootedFolders) {
        foreach (string tp in rootedFolders) {
            string nextPath = tp;
            nextPath = nextPath.Contains("%ROOT%") ? nextPath.Replace("%ROOT%", base.Root) : Path.Combine(base.Root, nextPath);
            base.AllFolders.Add(nextPath);
        }
        return this;
    }

    protected override bool ActualDoesFileExist(string filename) {
        return AllFiles.Contains(filename);
    }
}