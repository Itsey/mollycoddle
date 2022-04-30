namespace mollycoddle {
    using System;
    using System.Security.Cryptography;
    using Minimatch;


    public class FileStructureChecker : StructureCheckerBase {
        protected List<FileCheckEntity> ViolatedActions = new List<FileCheckEntity>();
        protected List<FileCheckEntity> Actions = new List<FileCheckEntity>();


        protected virtual string ValidateMasterPath(string pathToMaster) {

            ArgumentNullException.ThrowIfNull(pathToMaster);

            pathToMaster = pathToMaster.Replace("%MASTERROOT%", mo.MasterPath);

            if (!File.Exists(pathToMaster)) {
                throw new FileNotFoundException("master path must be present", pathToMaster);
            }
            
            return pathToMaster;
        }



        protected virtual string ValidateActualPath(string pathToActual) {
            return pathToActual.Replace("%ROOT%", ps.Root);
        }


        protected virtual string GetFileContents(string path) {
            return File.ReadAllText(path);
        }

        
        protected override CheckResult ActualExecuteChecks(CheckResult result) {

            foreach (var fn in ps.AllFiles) { 

                int i = 0;
                while (i < Actions.Count) {
                    var chk = Actions[i];

                    if (chk.IsInViolation) { 
                        continue; 
                    }

                    if (chk.ExecuteCheckWasViolation(fn)) {
                        ViolatedActions.Add(chk);
                        Actions.Remove(chk);
                    } else {
                        i++;
                    }
                }

            }
            
            // Loop to catch those violations which require to actively pass, e.g. Must exist or Must not exist, they 
            // need to check each file to determine if they have passed or not.
            foreach (var a in Actions) {
                if (!a.Passed) {
                    result.AddDefect(a.OwningRuleIdentity, a.AdditionalInfo);
                }
            }
            foreach (var l in ViolatedActions) {
                result.AddDefect(l.OwningRuleIdentity, l.AdditionalInfo);
                Actions.Add(l);
            }
            ViolatedActions.Clear();

            return result;

        }

       

        public FileStructureChecker(ProjectStructure ps, MollyOptions mo) : base(ps,mo){
            this.ps = ps;
        }

        protected override void AddFileValidator(FileValidationChecks fs) {
            base.AddFileValidator(fs);

            foreach(var l in fs.FilesThatMustMatchTheirMaster()) {
                AssignCompareWithMasterAction(ValidateActualPath(l.PatternForSourceFile), l.FullPathForMasterFile, fs.TriggeringRule);
            }
            foreach(var ln in fs.FilesThatMustExist()) {
                AssignMustExistAction(fs.TriggeringRule, ln);
            }
        }

        protected virtual Action<FileCheckEntity, string> GetContentsCheckerAction(string masterContentsPath) {

            var fi = new FileInfo(masterContentsPath);
            long masterLenght = fi.Length;
            using var fs = fi.OpenRead();
            var md5base = MD5.Create().ComputeHash(fs);

            var act = new Action<FileCheckEntity, string>((resultant, filenameToCheck) => {
                var f = new FileInfo(filenameToCheck);
                if (f.Length == masterLenght) {

                    using var fs = f.OpenRead();
                    var md5comp = MD5.Create().ComputeHash(fs);
                    if (!md5base.SequenceEqual(md5comp)) {
                        // Failed
                        resultant.IsInViolation = true;
                        resultant.AdditionalInfo = filenameToCheck;
                    }
                } else {
                    resultant.IsInViolation = true;
                    resultant.AdditionalInfo = $"{filenameToCheck} does not match master.";
                }
            });
            return act;
        }
        public virtual void AssignCompareWithMasterAction(string patternToMatch, string pathToMaster, string ruleName) {
            patternToMatch = ValidateActualPath(patternToMatch);
            string pm = ValidateMasterPath(pathToMaster);

            var fca = new FileCheckEntity(ruleName);
            fca.PerformCheck = GetContentsCheckerAction(pm);
            fca.DoesMatch = new Minimatcher(patternToMatch, o);

            Actions.Add(fca);
        }

        public void AssignMustExistAction(string ruleName, string patternForFile) {
            var fca = new FileCheckEntity(ruleName);
            // Must Exist defaults to having not passed.  It passes if the file is then found.
            patternForFile = ValidateActualPath(patternForFile);
            fca.Passed = false;
            fca.AdditionalInfo = $"{patternForFile} must exist.";
            fca.PerformCheck = GetMustExistChecker();
            fca.DoesMatch = new Minimatcher(patternForFile, o);


            Actions.Add(fca);
        }
        public void AssignFileMustNotContainAction(string violationRuleName, string patternForFile, string textToFind) {
            var fca = new FileCheckEntity(violationRuleName);
            fca.PerformCheck = GetFileContentsMustChecker(textToFind, false);
            fca.DoesMatch = new Minimatcher(patternForFile, o);
            Actions.Add(fca);
        }

    
        

        protected virtual Action<FileCheckEntity, string> GetMustExistChecker() {

            var act = new Action<FileCheckEntity, string>((resultant, filenameToCheck) => {
                if (!File.Exists(filenameToCheck)) {
                    resultant.IsInViolation = true;
                    resultant.AdditionalInfo = filenameToCheck;
                } else {
                    resultant.Passed = true;
                }
            });
            return act;
        }



        

        private Action<FileCheckEntity, string> GetFileContentsMustChecker(string text, bool mustOrMustNot) {
            var act = new Action<FileCheckEntity, string>((resultant, filenameToCheck) => {
                var f = GetFileContents(filenameToCheck);               
                resultant.IsInViolation = f.Contains(text, StringComparison.OrdinalIgnoreCase) != mustOrMustNot;
                resultant.AdditionalInfo = filenameToCheck;
            });
            return act;
        }
    }
}