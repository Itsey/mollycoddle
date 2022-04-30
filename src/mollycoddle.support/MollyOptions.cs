using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plisky.Plumbing;
using System.Threading.Tasks;

namespace mollycoddle {
    public class MollyOptions {

        public string MasterPath { get; set; }
        
        public string DirectoryToTarget { get; set; }

        public string RulesFile { get; set; }

        public MollyOptions() {
            DirectoryToTarget = @"C:\Files\Code\git\PliskyDiagnostics";
            MasterPath = @"C:\Files\Code\git\mollycoddle\src\_Dependencies\TestMasterPath\";
        }
    }
}
