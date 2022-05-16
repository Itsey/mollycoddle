namespace mollycoddle {

    using System.Collections.Generic;

    public class FilePresenceChecker {

        public CheckResult Check(string rootPath, string[] dirs) {
            rootPath = rootPath.ToLower();

            Dictionary<string, bool> filesThatMustExist = new Dictionary<string, bool>();
            filesThatMustExist.Add(Path.Combine(rootPath, ".gitignore"), false);

            foreach (var l in dirs) {
                var j = l.ToLower();
                if (filesThatMustExist.ContainsKey(j)) {
                    filesThatMustExist[j] = true;
                }
            }

            int failCount = 0;
            foreach (var k in filesThatMustExist.Keys) {
                if (!filesThatMustExist[k]) {
                    failCount++;
                }
            }

            return new CheckResult() {
                DefectCount = failCount
            };
        }
    }
}