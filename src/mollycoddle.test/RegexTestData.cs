using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mollycoddle.test {
    public class StartStopTestData {
        public string? StartString { get; set; }
        public string? StopString { get; set; }

        public List<Tuple<string, bool>> TestList { get; set; } = new List<Tuple<string, bool>>();
    }

    
}
