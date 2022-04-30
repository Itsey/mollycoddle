using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plisky.Plumbing;

namespace mollycoddle {


    [CommandLineArguments]
    public class MollyCommandLine {

        [CommandLineArg("masterRoot")]
        public string MasterPath { get; set; }

        [CommandLineArg("dir", IsRequired = true, IsSingleParameterDefault =true)]
        public string DirectoryToTarget { get; set; }

        public string RulesFile { get; set; }

        public MollyCommandLine() {
            
           // MasterPath = @"C:\Files\Code\git\mollycoddle\src\_Dependencies\TestMasterPath\";
        }

        public MollyOptions GetOptions() {
            var result = new MollyOptions();
            result.MasterPath = MasterPath;
            if (DirectoryToTarget.EndsWith("\\")) {
                result.DirectoryToTarget = DirectoryToTarget.Substring(0,DirectoryToTarget.Length-1);
            } else {
                result.DirectoryToTarget = DirectoryToTarget;
            }
            
            result.RulesFile = RulesFile;
            return result;
        }
    }
}
