namespace mollycoddle.test;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using mollycoddle;
using Plisky.Diagnostics;
using Shouldly;
using Xunit;

public class ProgramOutputTests {
    private static readonly object sync = new();

    [Fact]
    public void WriteOutputDefault_WritesExpectedPrefixes() {
        var method = GetProgramMethod("WriteOutputDefault");

        string output;
        lock (sync) {
            using var sw = new StringWriter();
            var original = Console.Out;
            Console.SetOut(sw);
            try {
                method.Invoke(null, ["boom", OutputType.Error]);
                method.Invoke(null, ["ok", OutputType.EndSuccess]);
                method.Invoke(null, ["fault", OutputType.Violation]);
            } finally {
                Console.SetOut(original);
            }
            output = sw.ToString();
        }
        output.ShouldContain("⚠  Error: boom");
        output.ShouldContain("😎  Completed.ok");
        output.ShouldContain("💩  Violation: fault");
    }

    [Fact]
    public void WriteOutputAzDo_UsesWarningPrefixWhenWarningModeEnabled() {
        var method = GetProgramMethod("WriteOutputAzDo");
        var warningModeField = GetProgramField("warningMode");
        string output;
        lock (sync) {
            warningModeField.SetValue(null, true);
            using var sw = new StringWriter();
            var original = Console.Out;
            Console.SetOut(sw);
            try {
                method.Invoke(null, ["violation", OutputType.Violation]);
                method.Invoke(null, ["error", OutputType.Error]);
            } finally {
                Console.SetOut(original);
                warningModeField.SetValue(null, false);
            }
            output = sw.ToString();
        }
        output.ShouldContain("type=warning");
    }

    [Fact]
    public void WriteEndMessage_WritesFixMessageAndFailureMessage() {
        var method = GetProgramMethod("WriteEndMessage");
        var writeOutputField = GetProgramField("writeOutput");
        var captured = new List<(string Message, OutputType Type)>();
        var original = (Action<string, OutputType>)writeOutputField.GetValue(null)!;
        writeOutputField.SetValue(null, new Action<string, OutputType>((m, t) => captured.Add((m, t))));

        var cr = new CheckResult();
        cr.AddDefect("rule", "details");
        var mo = new MollyOptions {
            Fix = true
        };
        var mm = new MollyMain(mo, new Bilge("test")) {
            fixApplied = true
        };

        try {
            method.Invoke(null, [cr, mm, mo, "elapsed"]);
        } finally {
            writeOutputField.SetValue(null, original);
        }

        captured.ShouldContain(c => c.Type == OutputType.Info && c.Message.Contains("Fix applied for supported violations"));
        captured.ShouldContain(c => c.Type == OutputType.EndFailure && c.Message.Contains("Total Violations 1"));
    }

    private static MethodInfo GetProgramMethod(string name) {
        var mi = typeof(Program).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        mi.ShouldNotBeNull();
        return mi!;
    }

    private static FieldInfo GetProgramField(string name) {
        var fi = typeof(Program).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        fi.ShouldNotBeNull();
        return fi!;
    }
}
