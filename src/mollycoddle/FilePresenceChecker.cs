namespace mollycoddle {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
