// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Reflection;
using mollycoddle;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Plumbing;

public class Program {
    private static Action<string, OutputType> writeOutput = WriteOutputDefault;
    private static bool warningMode = false;
    private static int exitCode = 0;
    private static string timingMessage = "";
    private static long lastCheckpoint = 0;

    private static async Task<int> Main(string[] args) {
        var sw = new Stopwatch();

        Hub.Current.UseStrongReferences = true;
        Hub.Current.LookFor<CheckpointMessage>((a) => {
            long time = sw.ElapsedMilliseconds - lastCheckpoint;
            lastCheckpoint = sw.ElapsedMilliseconds;
            timingMessage += $"({a.Name} : {time}ms.)";
        });

        Bilge? b = null;

        var clas = new CommandArgumentSupport {
            ArgumentPrefix = "-",
            ArgumentPostfix = "="
        };
        var ma = clas.ProcessArguments<MollyCommandLine>(args);
        warningMode = ma.WarningMode;

        if (string.Equals(ma.OutputFormat, "azdo", StringComparison.OrdinalIgnoreCase)) {
            writeOutput = WriteOutputAzDo;
        }

        WriteGreetingMessage();


        if (ma.Disabled) {
            writeOutput($"MollyCoddle is disabled, returning success.", OutputType.Verbose);
            exitCode = 0;
            goto TheEndIsNigh;
        }

        var mo = ma.GetOptions();
        if (mo.EnableDebug) {
            writeOutput($"Debug Mode {mo.DebugSetting}", OutputType.Info);
            ConfigureTrace(mo.DebugSetting);
        }

        b = new Bilge("mollycoddle");
        _ = Bilge.Alert.Online("mollycoddle");
        WriteDebuggingInfoOnStartup(args, ma, b);


        var mm = new MollyMain(mo, b);
        mm.WriteOutput = writeOutput;

        if ((ma.DirectoryToTarget == "?") || (ma.DirectoryToTarget == "/?") || (ma.DirectoryToTarget.ToLower() == "/help")) {
            Console.WriteLine("MollyCoddle, for when you cant let the babbers code on their own....");
            Console.WriteLine(clas.GenerateHelp(ma, "MollyCoddle"));
            exitCode = 0;
        } else {
            try {
                sw.Start();
                b.Verbose.Log("MC starts, about to execute molly checks");
                var cr = await mm.DoMollly();
                exitCode = cr.DefectCount;
                b.Verbose.Log($"MC completes {cr.DefectCount} defects identified.");
                sw.Stop();
                string elapsedString = $" Took {sw.ElapsedMilliseconds}ms. {timingMessage}";
                if (mo.GetCommonFiles) {
                    WriteGetEndMessage(cr, mo, elapsedString);
                } else if (cr.DefectCount == 0) {
                    writeOutput($"No Violations, Mollycoddle Pass. ({elapsedString})", OutputType.EndSuccess);
                } else {
                    writeOutput($"Total Violations {cr.DefectCount}.  {elapsedString}", warningMode ? OutputType.EndSuccess : OutputType.EndFailure);
                }
            } catch (Exception ex) {
                b.Error.Dump(ex, "exception captured during mc execution");
                writeOutput(ex.Message, OutputType.Error);
                exitCode = -3;
                goto TheEndIsNigh;
            }

            if (warningMode) {
                b.Info.Log("Warning mode, resetting exit code to zero");
                exitCode = 0;
            }
        }

    TheEndIsNigh:
        // Who doesnt love a good goto, secretly.
        b?.Verbose.Log("Mollycoddle, Exit");
        b?.Flush().Wait();
        return exitCode;
    }

    private static void WriteDebuggingInfoOnStartup(string[] args, MollyCommandLine ma, Bilge b) {
        string mver = Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "unknown";

        b.Verbose.Log($"Molly Startup Debug Info {mver}");
        b.Verbose.Dump(args, "command line arguments");
        b.Verbose.Log($"MolCommandLine : Rules : {ma.RulesFile}");
        b.Verbose.Log($"MolCommandLine : Primary : {ma.PrimaryPath}");
        b.Verbose.Log($"MolCommandLine : Directory : {ma.DirectoryToTarget}");
        b.Verbose.Log($"MolCommandLine : Disabled : {ma.Disabled}");

    }

    private static void ConfigureTrace(string debugSetting) {
        Bilge.Default.Assert.False(string.IsNullOrEmpty(debugSetting), "The debugSetting can not be empty at this point");
        Bilge.Default.ActiveTraceLevel = Bilge.SetConfigurationResolver(debugSetting)("default", Bilge.Default.ActiveTraceLevel);

#if DEBUG && true
        Bilge.AddHandler(new TCPHandler("127.0.0.1", 9060, true));
#endif
        Bilge.AddHandler(new ConsoleHandler());
    }

    private static void WriteGreetingMessage() {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        string verString = Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "unknown";
        writeOutput($"🚼 MollyCoddle Online. ({verString})", OutputType.Info);
    }

    private static void WriteOutputDefault(string v, OutputType ot) {
        string pfx = "";
        switch (ot) {
            case OutputType.Violation: pfx = "💩  Violation: "; break;
            case OutputType.Error: pfx = "⚠  Error: "; break;
            case OutputType.Info: pfx = "Info: "; break;
            case OutputType.EndSuccess: pfx = "😎  Completed."; break;
            case OutputType.EndFailure: pfx = "😢  Completed."; break;
        }
        Console.WriteLine($"{pfx}{v}");
    }

    private static void WriteOutputAzDo(string v, OutputType ot) {
        string pfx = "";
        string errType = warningMode ? "warning" : "error";

        switch (ot) {
            case OutputType.Violation: pfx = $"##vso[task.logissue type={errType}] 💩 Violation: "; break;
            case OutputType.Error: pfx = $"##vso[task.logissue type={errType}] ⚠ Error: "; break;
            case OutputType.Info: pfx = "##[command]"; break;
            case OutputType.EndSuccess:
                Console.WriteLine($"{v}");
                pfx = "##vso[task.complete result=Succeeded;]";
                break;

            case OutputType.EndFailure:
                Console.WriteLine($"{v}");
                pfx = "##vso[task.complete result=Failed;]";
                break;
        }
        Console.WriteLine($"{pfx}{v}");
    }
    private static void WriteGetEndMessage(CheckResult cr, MollyOptions mo, string elapsedString) {
        if (cr.DefectCount == 0) {
            writeOutput($" Common files saved successfully to {mo.DirectoryToTarget}. ({elapsedString})", OutputType.EndSuccess);
        } else {
            writeOutput($" {cr.DefectCount} errors occurred while fetching common files. ({elapsedString})", OutputType.EndFailure);
        }
    }
}