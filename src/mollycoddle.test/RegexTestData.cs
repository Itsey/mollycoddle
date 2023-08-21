namespace mollycoddle.test;

using System;
using System.Collections.Generic;

public class StartStopTestData {
    public string? StartString { get; set; }
    public string? StopString { get; set; }

    public List<Tuple<string, bool>> TestList { get; set; } = new List<Tuple<string, bool>>();
}