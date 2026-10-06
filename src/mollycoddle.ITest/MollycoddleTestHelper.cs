using System.Diagnostics;
using System.Text;
using Shouldly;

namespace mollycoddle.ITest;

internal sealed record MollycoddleExecutionResult(
    int ExitCode, string StdOut, string StdErr, string Invocation) {
    public string Diagnostics =>
        $"{Invocation}{Environment.NewLine}Exit code: {ExitCode}{Environment.NewLine}" +
        $"stdout:{Environment.NewLine}{StdOut}{Environment.NewLine}stderr:{Environment.NewLine}{StdErr}";

    public void AssertScan(int expectedExitCode, params string[] expectedRuleNames) {
        ExitCode.ShouldBe(expectedExitCode, Diagnostics);
        StdErr.ShouldBeEmpty(Diagnostics);
        string[] violations = [.. StdOut.Split('\n')
            .Where(line => line.Contains("Violation: ", StringComparison.Ordinal))
            .Select(line => line[(line.IndexOf("Violation: ", StringComparison.Ordinal) + "Violation: ".Length)..]
                .TrimStart().Split(" (", StringSplitOptions.None)[0])
            .Order(StringComparer.Ordinal)];
        violations.ShouldBe([.. expectedRuleNames.Order(StringComparer.Ordinal)], Diagnostics);
        StdOut.ShouldContain(expectedRuleNames.Length == 0
            ? "No Violations, Mollycoddle Pass."
            : $"Total Violations {expectedRuleNames.Length}.", Case.Sensitive, Diagnostics);
    }
}

internal static class MollycoddleTestHelper {
    public static Task<MollycoddleExecutionResult> ScanAsync(
        TestWorkspace workspace, string rulesFile, params string[] extraArguments) {
        return ExecuteAsync(workspace.Repository, [
            $"-dir={workspace.Repository}",
            $"-rulesfile={rulesFile}",
            $"-temppath={workspace.CacheDirectory}",
            .. extraArguments
        ]);
    }

    public static async Task<MollycoddleExecutionResult> ExecuteAsync(
        string workingDirectory, string[] arguments, TimeSpan? timeout = null) {
        string assemblyPath = Path.Combine(AppContext.BaseDirectory, "cli", "mollycoddle.dll");
        if (!File.Exists(assemblyPath)) {
            throw new FileNotFoundException("Build the integration project to stage the Mollycoddle CLI.", assemblyPath);
        }

        var startInfo = new ProcessStartInfo {
            FileName = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(assemblyPath);
        foreach (string argument in arguments) {
            startInfo.ArgumentList.Add(argument);
        }
        string invocation = $"Working directory: {workingDirectory}{Environment.NewLine}" +
            $"Command: {startInfo.FileName} {string.Join(" ", startInfo.ArgumentList.Select(a => $"\"{a}\""))}";

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start()) {
            throw new InvalidOperationException($"Unable to start Mollycoddle.{Environment.NewLine}{invocation}");
        }
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var cancellation = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(60));
        try {
            cancellation.Token.ThrowIfCancellationRequested();
            await process.WaitForExitAsync(cancellation.Token);
        } catch (OperationCanceledException) when (cancellation.IsCancellationRequested) {
            try {
                if (!process.HasExited) {
                    process.Kill(entireProcessTree: true);
                }
            } catch (InvalidOperationException) when (process.HasExited) {
                // The process can exit between checking HasExited and killing it.
            }
            await process.WaitForExitAsync();
            throw new TimeoutException($"Mollycoddle timed out. Process ID: {process.Id}{Environment.NewLine}{invocation}" +
                $"{Environment.NewLine}stdout:{Environment.NewLine}{await stdout}" +
                $"{Environment.NewLine}stderr:{Environment.NewLine}{await stderr}");
        }

        return new MollycoddleExecutionResult(process.ExitCode, await stdout, await stderr, invocation);
    }
}
