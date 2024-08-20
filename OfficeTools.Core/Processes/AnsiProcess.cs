﻿using System.Diagnostics;
using System.Text;

namespace OfficeTools.Core.Processes;

/// <summary>
///     Process supporting parsing of ANSI escape sequences
/// </summary>
public class AnsiProcess : Process
{
    private AsyncStreamReader? stderrReader;
    private AsyncStreamReader? stdoutReader;

    public AnsiProcess(ProcessStartInfo startInfo)
    {
        StartInfo = startInfo;
        EnableRaisingEvents = false;

        StartInfo.UseShellExecute = false;
        StartInfo.CreateNoWindow = true;
        StartInfo.RedirectStandardOutput = true;
        StartInfo.RedirectStandardInput = true;
        StartInfo.RedirectStandardError = true;

        // Need this to parse ANSI escape sequences correctly
        StartInfo.StandardOutputEncoding = new UTF8Encoding(false);
        StartInfo.StandardErrorEncoding = new UTF8Encoding(false);
        StartInfo.StandardInputEncoding = new UTF8Encoding(false);
    }

    /// <summary>
    ///     Start asynchronous reading of stdout and stderr
    /// </summary>
    /// <param name="callback">Called on each new line</param>
    public void BeginAnsiRead(Action<ProcessOutput> callback)
    {
        Stream stdoutStream = StandardOutput.BaseStream;
        stdoutReader = new AsyncStreamReader(
            stdoutStream,
            s =>
            {
                if (s == null)
                    return;

                callback(ProcessOutput.FromStdOutLine(s));
            },
            StandardOutput.CurrentEncoding
        );

        Stream stderrStream = StandardError.BaseStream;
        stderrReader = new AsyncStreamReader(
            stderrStream,
            s =>
            {
                if (s == null)
                    return;

                callback(ProcessOutput.FromStdErrLine(s));
            },
            StandardError.CurrentEncoding
        );

        stdoutReader.BeginReadLine();
        stderrReader.BeginReadLine();
    }

    /// <summary>
    ///     Waits for output readers to finish
    /// </summary>
    public async Task WaitUntilOutputEOF(CancellationToken ct = default)
    {
        if (stdoutReader is not null)
        {
            await stdoutReader.EOF.WaitAsync(ct).ConfigureAwait(false);
        }

        if (stderrReader is not null)
        {
            await stderrReader.EOF.WaitAsync(ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Cancels stream readers, no effect if already cancelled
    /// </summary>
    public void CancelStreamReaders()
    {
        stdoutReader?.CancelOperation();
        stderrReader?.CancelOperation();
    }

    protected override void Dispose(bool disposing)
    {
        CancelStreamReaders();
        stdoutReader?.Dispose();
        stdoutReader = null;
        stderrReader?.Dispose();
        stderrReader = null;
        base.Dispose(disposing);
    }
}