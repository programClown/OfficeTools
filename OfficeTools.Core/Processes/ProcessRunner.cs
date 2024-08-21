﻿using System.Diagnostics;
using System.Text;
using NLog;
using OfficeTools.Core.Exceptions;
using OfficeTools.Core.Helper;

namespace OfficeTools.Core.Processes;

public static class ProcessRunner
{
    readonly private static Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///     Opens the given URL in the default browser.
    /// </summary>
    /// <param name="url">URL as string</param>
    public static void OpenUrl(string url)
    {
        Logger.Debug($"Opening URL '{url}'");
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    /// <summary>
    ///     Opens the given URL in the default browser.
    /// </summary>
    /// <param name="url">URI, using AbsoluteUri component</param>
    public static void OpenUrl(Uri url)
    {
        OpenUrl(url.AbsoluteUri);
    }

    /// <summary>
    ///     Start an executable or .app on macOS.
    /// </summary>
    public static Process StartApp(string path, ProcessArgs args)
    {
        var startInfo = new ProcessStartInfo();

        if (Compat.IsMacOS)
        {
            startInfo.FileName = "open";
            startInfo.Arguments = args.Prepend([path, "--args"]).ToString();
            startInfo.UseShellExecute = true;
        }
        else
        {
            startInfo.FileName = path;
            startInfo.Arguments = args;
        }

        return Process.Start(startInfo) ?? throw new NullReferenceException("Process.Start returned null");
    }

    /// <summary>
    ///     Opens the given folder in the system file explorer.
    /// </summary>
    public static async Task OpenFolderBrowser(string directoryPath)
    {
        if (Compat.IsWindows)
        {
            // Ensure path ends in DirectorySeparatorChar to unambiguously point to a directory
            if (!directoryPath.EndsWith(Path.DirectorySeparatorChar))
            {
                directoryPath += Path.DirectorySeparatorChar;
            }

            using var process = new Process();

            process.StartInfo.FileName = Quote(directoryPath);
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "open";

            process.Start();

            // Apparently using verb open detaches the process object, so we can't wait for process exit here
        }
        else if (Compat.IsLinux)
        {
            using var process = new Process();
            process.StartInfo.FileName = directoryPath;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            await process.WaitForExitAsync().ConfigureAwait(false);
        }
        else if (Compat.IsMacOS)
        {
            using var process = new Process();
            process.StartInfo.FileName = "open";
            process.StartInfo.Arguments = Quote(directoryPath);
            process.Start();
            await process.WaitForExitAsync().ConfigureAwait(false);
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }

    /// <summary>
    ///     Opens the given file within its folder in the system file explorer.
    /// </summary>
    public static async Task OpenFileBrowser(string filePath)
    {
        if (Compat.IsWindows)
        {
            using var process = new Process();
            process.StartInfo.FileName = "explorer.exe";
            process.StartInfo.Arguments = $"/select, {Quote(filePath)}";
            process.Start();
            await process.WaitForExitAsync().ConfigureAwait(false);
        }
        else if (Compat.IsLinux)
        {
            using var process = new Process();
            process.StartInfo.FileName = "dbus-send";
            process.StartInfo.Arguments =
                "--print-reply --dest=org.freedesktop.FileManager1 "
                + "/org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems "
                + $"array:string:\"file://{filePath}\" string:\"\"";

            process.StartInfo.UseShellExecute = true;
            process.Start();
            await process.WaitForExitAsync().ConfigureAwait(false);
        }
        else if (Compat.IsMacOS)
        {
            using var process = new Process();
            process.StartInfo.FileName = "open";
            process.StartInfo.Arguments = $"-R {Quote(filePath)}";
            process.Start();
            await process.WaitForExitAsync().ConfigureAwait(false);
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }

    /// <summary>
    ///     Starts and tracks a process.
    /// </summary>
    private static Process StartTrackedProcess(Process process)
    {
        process.Start();

        // Currently only supported on Windows
        if (Compat.IsWindows)
        {
            ProcessTracker.AddProcess(process);
        }

        return process;
    }

    public static async Task<string> GetProcessOutputAsync(
        string fileName,
        string arguments,
        string? workingDirectory = null,
        Dictionary<string, string>? environmentVariables = null
    )
    {
        Logger.Debug($"Starting process '{fileName}' with arguments '{arguments}'");

        var info = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        if (environmentVariables != null)
        {
            foreach (var (key, value) in environmentVariables)
            {
                info.EnvironmentVariables[key] = value;
            }
        }

        if (workingDirectory != null)
        {
            info.WorkingDirectory = workingDirectory;
        }

        using var process = new Process();
        process.StartInfo = info;
        StartTrackedProcess(process);

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return output;
    }

    public static async Task<ProcessResult> GetProcessResultAsync(
        string fileName,
        ProcessArgs arguments,
        string? workingDirectory = null,
        IReadOnlyDictionary<string, string>? environmentVariables = null
    )
    {
        Logger.Debug($"Starting process '{fileName}' with arguments '{arguments}'");

        var info = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (environmentVariables != null)
        {
            foreach (var (key, value) in environmentVariables)
            {
                info.EnvironmentVariables[key] = value;
            }
        }

        if (workingDirectory != null)
        {
            info.WorkingDirectory = workingDirectory;
        }

        using var process = new Process();
        process.StartInfo = info;
        StartTrackedProcess(process);

        var stdout = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        var stderr = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);

        await process.WaitForExitAsync().ConfigureAwait(false);

        string? processName = null;
        TimeSpan elapsed = default;

        // Accessing these properties may throw an exception if the process has already exited
        try
        {
            processName = process.ProcessName;
        }
        catch (SystemException)
        {
        }

        try
        {
            elapsed = process.ExitTime - process.StartTime;
        }
        catch (SystemException)
        {
        }

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = stdout,
            StandardError = stderr,
            ProcessName = processName,
            Elapsed = elapsed
        };
    }

    public static Process StartProcess(
        string fileName,
        string arguments,
        string? workingDirectory = null,
        Action<string?>? outputDataReceived = null,
        IReadOnlyDictionary<string, string>? environmentVariables = null
    )
    {
        Logger.Debug($"Starting process '{fileName}' with arguments '{arguments}'");
        var info = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (environmentVariables != null)
        {
            foreach (var (key, value) in environmentVariables)
            {
                info.EnvironmentVariables[key] = value;
            }
        }

        if (workingDirectory != null)
        {
            info.WorkingDirectory = workingDirectory;
        }

        var process = new Process { StartInfo = info };
        StartTrackedProcess(process);

        if (outputDataReceived == null)
            return process;

        process.OutputDataReceived += (sender, args) => outputDataReceived(args.Data);
        process.ErrorDataReceived += (sender, args) => outputDataReceived(args.Data);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    public static AnsiProcess StartAnsiProcess(
        string fileName,
        string arguments,
        string? workingDirectory = null,
        Action<ProcessOutput>? outputDataReceived = null,
        IReadOnlyDictionary<string, string>? environmentVariables = null
    )
    {
        Logger.Debug(
            $"Starting process '{fileName}' with arguments '{arguments}' in working directory '{workingDirectory}'"
        );

        var info = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (environmentVariables != null)
        {
            foreach (var (key, value) in environmentVariables)
            {
                info.EnvironmentVariables[key] = value;
            }
        }

        if (workingDirectory != null)
        {
            info.WorkingDirectory = workingDirectory;
        }

        var process = new AnsiProcess(info);
        StartTrackedProcess(process);

        if (outputDataReceived != null)
        {
            process.BeginAnsiRead(outputDataReceived);
        }

        return process;
    }

    public static AnsiProcess StartAnsiProcess(
        string fileName,
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        Action<ProcessOutput>? outputDataReceived = null,
        Dictionary<string, string>? environmentVariables = null
    )
    {
        // Quote arguments containing spaces
        var args = string.Join(" ", arguments.Where(s => !string.IsNullOrEmpty(s)).Select(Quote));
        return StartAnsiProcess(fileName, args, workingDirectory, outputDataReceived, environmentVariables);
    }

    public static async Task<ProcessResult> RunBashCommand(
        string command,
        string workingDirectory = "",
        IReadOnlyDictionary<string, string>? environmentVariables = null
    )
    {
        // Escape any single quotes in the command
        var escapedCommand = command.Replace("\"", "\\\"");
        var arguments = $"-c \"{escapedCommand}\"";

        Logger.Info($"Running bash command [bash {arguments}]");

        var processInfo = new ProcessStartInfo("bash", arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory
        };

        if (environmentVariables != null)
        {
            foreach (var (key, value) in environmentVariables)
            {
                processInfo.EnvironmentVariables[key] = value;
            }
        }

        using var process = new Process();
        process.StartInfo = processInfo;

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        process.OutputDataReceived += (_, args) => stdout.Append(args.Data);
        process.ErrorDataReceived += (_, args) => stderr.Append(args.Data);

        StartTrackedProcess(process);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync().ConfigureAwait(false);

        return new ProcessResult
        {
            ExitCode = process.ExitCode, StandardOutput = stdout.ToString(), StandardError = stderr.ToString()
        };
    }

    public static Task<ProcessResult> RunBashCommand(
        IEnumerable<string> commands,
        string workingDirectory = "",
        IReadOnlyDictionary<string, string>? environmentVariables = null
    )
    {
        // Quote arguments containing spaces
        var args = string.Join(" ", commands.Select(Quote));
        return RunBashCommand(args, workingDirectory, environmentVariables);
    }

    /// <summary>
    ///     Quotes argument with double quotes if it contains spaces,
    ///     and does not already start and end with double quotes.
    /// </summary>
    public static string Quote(string argument)
    {
        var inner = argument.Trim('"');
        return inner.Contains(' ') ? $"\"{inner}\"" : argument;
    }

    /// <summary>
    ///     Waits for process to exit, then validates exit code.
    /// </summary>
    /// <param name="process">Process to check.</param>
    /// <param name="expectedExitCode">Expected exit code.</param>
    /// <param name="cancelToken">Cancellation token.</param>
    /// <exception cref="ProcessException">Thrown if exit code does not match expected value.</exception>
    public static async Task WaitForExitConditionAsync(
        Process process,
        int expectedExitCode = 0,
        CancellationToken cancelToken = default
    )
    {
        if (!process.HasExited)
        {
            await process.WaitForExitAsync(cancelToken).ConfigureAwait(false);
        }

        if (process.ExitCode == expectedExitCode)
        {
            return;
        }

        // Accessing ProcessName may error on some platforms
        string? processName = null;
        try
        {
            processName = process.ProcessName;
        }
        catch (SystemException)
        {
        }

        throw new ProcessException(
            "Process "
            + (processName == null ? "" : processName + " ")
            + $"failed with exit-code {process.ExitCode}."
        );
    }
}