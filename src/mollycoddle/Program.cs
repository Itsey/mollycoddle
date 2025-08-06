// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
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

        writeOutput("MollyCoddle Online", OutputType.Info);

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

        if (ma.GetCommonFiles) {
            b = new Bilge("mollycoddle-get");
            if (mo.EnableDebug) {
                ConfigureTrace(mo.DebugSetting);
            }
            var fetcher = new CommonFilesFetcher(mo, b);
            int getResult = await fetcher.FetchCommonFilesAsync(ma.DirectoryToTarget, ma.PrimaryPath);
            if (getResult == 0) {
                writeOutput($"Common files fetched successfully for repository root '{ma.DirectoryToTarget}'.", OutputType.EndSuccess);
                exitCode = 0;
            } else {
                writeOutput($"One or more files failed to download for repository root '{ma.DirectoryToTarget}'.", OutputType.EndFailure);
                exitCode = -getResult;
            }
            goto TheEndIsNigh;
        }

        b = new Bilge("mollycoddle");
        _ = Bilge.Alert.Online("mollycoddle");
        b.Verbose.Dump(args, "command line arguments");

        var mm = new MollyMain(mo, b);
        mm.WriteOutput = writeOutput;

        if ((ma.DirectoryToTarget == "?") || (ma.DirectoryToTarget == "/?") || (ma.DirectoryToTarget.ToLower() == "/help")) {
            Console.WriteLine("MollyCoddle, for when you cant let the babbers code on their own....");
            Console.WriteLine(clas.GenerateHelp(ma, "MollyCoddle"));
            exitCode = 0;
        } else {
            try {
                sw.Start();

                var cr = await mm.DoMollly();
                exitCode = cr.DefectCount;

                sw.Stop();
                string elapsedString = $" Took {sw.ElapsedMilliseconds}ms. {timingMessage}";
                if (cr.DefectCount == 0) {
                    writeOutput($"No Violations, Mollycoddle Pass. ({elapsedString})", OutputType.EndSuccess);
                } else {
                    writeOutput($"Total Violations {cr.DefectCount}.  {elapsedString}", warningMode ? OutputType.EndSuccess : OutputType.EndFailure);
                }
            } catch (Exception ex) {
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

    private static void ConfigureTrace(string debugSetting) {
        Bilge.Default.Assert.False(string.IsNullOrEmpty(debugSetting), "The debugSetting can not be empty at this point");
        // TODO: Currently hardcoded trace, move this to configuration.
        Bilge.SetConfigurationResolver(debugSetting);

#if DEBUG && true
        Bilge.SetConfigurationResolver((x, y) => {
            return System.Diagnostics.SourceLevels.Verbose;
        });
        Bilge.AddHandler(new TCPHandler("127.0.0.1", 9060, true));

#endif
        Bilge.AddHandler(new ConsoleHandler());
    }

    private static void WriteOutputDefault(string v, OutputType ot) {
        string pfx = "";
        switch (ot) {
            case OutputType.Violation: pfx = "💩 Violation: "; break;
            case OutputType.Error: pfx = "⚠ Error: "; break;
            case OutputType.Info: pfx = "Info: "; break;
            case OutputType.EndSuccess: pfx = "😎 Completed."; break;
            case OutputType.EndFailure: pfx = "😢 Completed."; break;
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
}