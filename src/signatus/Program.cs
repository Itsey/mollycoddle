using System.Diagnostics;
using System.Text;
using Minimatch;
using Plisky.Diagnostics;
using Plisky.Plumbing;
using signatus;

internal class Program {

    private static async Task<int> Main(string[] args) {
        CommandArgumentSupport clas = new CommandArgumentSupport();
        var o = clas.ProcessArguments<SignatusOptions>(args);

        Bilge.Alert.Online("Signatus");
        Bilge b = new Bilge();

        var scriptExecution = @"C:\Files\Code\nosccm\mollycoddle\_Dependencies\test.ps1";
        Options mo = new Options() { AllowWindowsPaths = true, IgnoreCase = true };

        List<Func<string, bool>> checks = new List<Func<string, bool>>();

        if (!File.Exists(o.InputFile)) {
            Console.WriteLine("Input file missing");
            return 3;
        }

        var mms = File.ReadAllLines(o.InputFile);
        foreach (var m in mms) {
            if (m.Trim().StartsWith("#")) {
                continue;
            }

            checks.Add(Minimatcher.CreateFilter(m, mo));
        }

        var to = new StringBuilder();

        var fso = Directory.GetFileSystemEntries(o.Directory);
        foreach (var f in fso) {
            bool shouldScan = false;
            foreach (var l in checks) {
                if (l(f)) {
                    shouldScan = true;
                    break;
                }
            }

            if (!shouldScan) {
                continue;
            }

            var ts = $"{scriptExecution} {f}";
            var psCommandBytes = System.Text.Encoding.Unicode.GetBytes(ts);
            var psCommandBase64 = Convert.ToBase64String(psCommandBytes);

            var start = new ProcessStartInfo {
                FileName = "powershell.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = $"-NoProfile -ExecutionPolicy unrestricted -EncodedCommand {psCommandBase64}",
                CreateNoWindow = true
            };

            using var p = Process.Start(start);

            if (p == null) {
                Console.WriteLine("Error starting process");
                return 1;
            }

            using var reader = p.StandardOutput;
            p.EnableRaisingEvents = true;
            var stdOutput = reader.ReadToEnd();
            to.Append(stdOutput);
            await p.WaitForExitAsync();
            if (p.ExitCode != 0) {
                Console.WriteLine("Non zero exit code");
                return 2;
            }
        }

        Console.WriteLine(to);

        return 0;
    }
}