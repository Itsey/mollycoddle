namespace mollycoddle.test {
    using System;
    using System.Security.Cryptography;
    using Minimatch;

    public class FileAction {
        public FileAction() {
        }

        public Action<string> PerformCheck { get; set; }

        public string ViolationRuleName { get; set; }

        public bool IsInViolation { get; set; }
        public string AdditionalInfo { get; set; }
        public bool FailedAlready { get; internal set; }
    }


    public class FileContentsChecker {
        private Options o = new Options() { AllowWindowsPaths = true, IgnoreCase = true };
        protected ProjectStructure ps;
        protected List<FileAction> ViolatedActions = new List<FileAction>();
        protected List<FileAction> Actions = new List<FileAction>();

        

        protected virtual string[] GetFileContents(string path) {
            return File.ReadAllLines(path);
        }


        public CheckResult CheckFiles() {

            foreach (var fn in ps.AllFiles) {

                int i = 0;
                while (i < Actions.Count) {
                    var chk = Actions[i];
                    chk.PerformCheck(fn);
                    if (chk.IsInViolation) {
                        ViolatedActions.Add(chk);
                        Actions.Remove(chk);
                    } else {
                        i++;
                    }
                }

            }

            var result = new CheckResult();
            foreach (var l in ViolatedActions) {
                result.AddDefect(l.ViolationRuleName,l.AdditionalInfo);
                Actions.Add(l);
            }
            ViolatedActions.Clear();

            return result;

        }

        public FileContentsChecker(ProjectStructure ps) {
            this.ps = ps;
        }
    

        public void CompareWithMaster(string patternToMatch, string pathToMaster) {
            var act = new FileAction();
            string pm = ValidateMasterPath(pathToMaster);
            FileInfo fi = new FileInfo(pm);
            long masterLenght = fi.Length;
            using var fs = fi.OpenRead();
            var md5base = MD5.Create().ComputeHash(fs);

            Minimatcher m = new Minimatcher(patternToMatch, o);
            act.PerformCheck = new Action<string>(filenameToCheck => {
                if (act.IsInViolation) { return; }

                if (m.IsMatch(filenameToCheck)) {
                    FileInfo f = new FileInfo(filenameToCheck);
                    if (f.Length == masterLenght) {
                        
                        using var fs = f.OpenRead();
                        var md5comp = MD5.Create().ComputeHash(fs);
                        if (!md5base.SequenceEqual(md5comp)) {
                            // Failed
                            
                            act.IsInViolation = true;
                            act.AdditionalInfo = filenameToCheck;
                        }
                    } else {
                        act.IsInViolation = true;
                        act.AdditionalInfo = filenameToCheck;
                    }
                    
                }
            });
            Actions.Add(act);
        }

        private string ValidateMasterPath(string pathToMaster) {
            return pathToMaster;
        }

        public void MustExist(string patternForFile) {
            var act = new FileAction();
            Actions.Add(act);
        }

        public void MustNotExist(string patternForFile, params string[] exceptWhere) {
            var act = new FileAction();
            Actions.Add(act);

        }

        public void MustNotContain(string patternForFile, string textToFind) {
            var act = new FileAction();
            Actions.Add(act);
        }
    }
}