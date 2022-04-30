namespace mollycoddle {
    using System;
    using Minimatch;

    public class FileCheckEntity : CheckEntity {
        public FileCheckEntity(string owningRuleName) : base(owningRuleName) {
        }


        public Action<FileCheckEntity, string> PerformCheck { get; set; }

        public bool ExecuteCheckWasViolation(string fileName) {
            if (IsInViolation) {  return false; }

            if (DoesMatch.IsMatch(fileName)) {
                PerformCheck(this,fileName);
            }
            return IsInViolation;
        }
        public string AdditionalInfo { get; set; }
       
        public Minimatcher DoesMatch { get; internal set; }
    }
}