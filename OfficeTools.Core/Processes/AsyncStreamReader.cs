﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using OfficeTools.Core.Extensions;

namespace OfficeTools.Core.Processes;

/// <summary>
///     Modified from System.Diagnostics.AsyncStreamReader to support terminal processing.
///     Currently has these modifications:
///     - Carriage returns do not count as newlines '\r'.
///     - APC messages are sent immediately without needing a newline.
///     <seealso cref="ApcParser" />
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class AsyncStreamReader : IDisposable
{
    private const int DefaultBufferSize = 1024; // Byte buffer size
    readonly private byte[] _byteBuffer;
    readonly private char[] _charBuffer;

    readonly private CancellationTokenSource _cts;
    readonly private Decoder _decoder;
    readonly private Queue<string?> _messageQueue;

    readonly private Stream _stream;

    // Delegate to call user function.
    readonly private Action<string?> _userCallBack;
    private bool _bLastCarriageReturn;
    private bool _cancelOperation;

    // Cache the last position scanned in sb when searching for lines.
    private int _currentLinePos;
    private Task? _readToBufferTask;
    private StringBuilder? _sb;

    // (new) Flag to send next buffer immediately
    private bool _sendNextBufferImmediately;

    // Creates a new AsyncStreamReader for the given stream. The
    // character encoding is set by encoding and the buffer size,
    // in number of 16-bit characters, is set by bufferSize.
    internal AsyncStreamReader(Stream stream, Action<string?> callback, Encoding encoding)
    {
        Debug.Assert(stream != null && encoding != null && callback != null, "Invalid arguments!");
        Debug.Assert(stream.CanRead, "Stream must be readable!");

        _stream = stream;
        _userCallBack = callback;
        _decoder = encoding.GetDecoder();
        _byteBuffer = new byte[DefaultBufferSize];

        // This is the maximum number of chars we can get from one iteration in loop inside ReadBuffer.
        // Used so ReadBuffer can tell when to copy data into a user's char[] directly, instead of our internal char[].
        var maxCharsPerBuffer = encoding.GetMaxCharCount(DefaultBufferSize);
        _charBuffer = new char[maxCharsPerBuffer];

        _cts = new CancellationTokenSource();
        _messageQueue = new Queue<string?>();
    }

    internal Task EOF => _readToBufferTask ?? Task.CompletedTask;

    public void Dispose()
    {
        _cts.Cancel();
    }

    // User calls BeginRead to start the asynchronous read
    internal void BeginReadLine()
    {
        _cancelOperation = false;

        if (_sb == null)
        {
            _sb = new StringBuilder(DefaultBufferSize);
            _readToBufferTask = Task.Run((Func<Task>)ReadBufferAsync);
        }
        else
        {
            FlushMessageQueue(false);
        }
    }

    internal void CancelOperation()
    {
        _cancelOperation = true;
    }

    // This is the async callback function. Only one thread could/should call this.
    private async Task ReadBufferAsync()
    {
        while (true)
        {
            try
            {
                var bytesRead = await _stream
                    .ReadAsync(new Memory<byte>(_byteBuffer), _cts.Token)
                    .ConfigureAwait(false);

                if (bytesRead == 0)
                    break;

                var charLen = _decoder.GetChars(_byteBuffer, 0, bytesRead, _charBuffer, 0);

                Debug.WriteLine(
                    $"AsyncStreamReader - Read {charLen} chars: "
                    + $"{new string(_charBuffer, 0, charLen).ToRepr()}"
                );

                _sb!.Append(_charBuffer, 0, charLen);
                MoveLinesFromStringBuilderToMessageQueue();
            }
            catch (IOException)
            {
                // We should ideally consume errors from operations getting cancelled
                // so that we don't crash the unsuspecting parent with an unhandled exc.
                // This seems to come in 2 forms of exceptions (depending on platform and scenario),
                // namely OperationCanceledException and IOException (for errorcode that we don't
                // map explicitly).
                break; // Treat this as EOF
            }
            catch (OperationCanceledException)
            {
                // We should consume any OperationCanceledException from child read here
                // so that we don't crash the parent with an unhandled exc
                break; // Treat this as EOF
            }

            // If user's delegate throws exception we treat this as EOF and
            // completing without processing current buffer content
            if (FlushMessageQueue(true))
            {
                return;
            }
        }

        // We're at EOF, process current buffer content and flush message queue.
        lock (_messageQueue)
        {
            if (_sb!.Length != 0)
            {
                var remaining = _sb.ToString();
                _messageQueue.Enqueue(remaining);
                _sb.Length = 0;
                Debug.WriteLine(
                    $"AsyncStreamReader - Reached EOF, sent remaining buffer: {remaining}"
                );
            }

            _messageQueue.Enqueue(null);
        }

        FlushMessageQueue(true);
    }

    // Send remaining buffer
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SendRemainingBuffer()
    {
        lock (_messageQueue)
        {
            if (_sb!.Length == 0)
                return;

            _messageQueue.Enqueue(_sb.ToString());
            _sb.Length = 0;
        }
    }

    // Send remaining buffer from index
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SendRemainingBuffer(int startIndex)
    {
        lock (_messageQueue)
        {
            if (_sb!.Length == 0)
                return;

            _messageQueue.Enqueue(_sb.ToString(startIndex, _sb.Length - startIndex));
            _sb.Length = 0;
        }
    }

    // Sends a message to the queue if not null or empty
    private void SendToQueue(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        lock (_messageQueue)
        {
            _messageQueue.Enqueue(message);
        }
    }

    // Read lines stored in StringBuilder and the buffer we just read into.
    // A line is defined as a sequence of characters followed by
    // a carriage return ('\r'), a line feed ('\n'), or a carriage return
    // immediately followed by a line feed. The resulting string does not
    // contain the terminating carriage return and/or line feed. The returned
    // value is null if the end of the input stream has been reached.
    private void MoveLinesFromStringBuilderToMessageQueue()
    {
        var currentIndex = _currentLinePos;
        var lineStart = 0;
        var len = _sb!.Length;

        // skip a beginning '\n' character of new block if last block ended
        // with '\r'
        if (_bLastCarriageReturn && len > 0 && _sb[0] == '\n')
        {
            currentIndex = 1;
            lineStart = 1;
            _bLastCarriageReturn = false;
        }

        while (currentIndex < len)
        {
            var ch = _sb[currentIndex];
            // Note the following common line feed chars:
            // \n - UNIX   \r\n - DOS
            switch (ch)
            {
                case '\n':
                {
                    // Include the '\n' as part of line.
                    var line = _sb.ToString(lineStart, currentIndex - lineStart + 1);
                    lineStart = currentIndex + 1;

                    lock (_messageQueue)
                    {
                        _messageQueue.Enqueue(line);
                    }

                    break;
                }
                // \r\n - Windows
                // \r alone is parsed as carriage return
                case '\r':
                {
                    // when next char is \n, we found \r\n - linebreak
                    if (currentIndex + 1 < len && _sb[currentIndex + 1] == '\n')
                    {
                        // Include the '\r\n' as part of line.
                        var line = _sb.ToString(lineStart, currentIndex - lineStart + 2);
                        lock (_messageQueue)
                        {
                            _messageQueue.Enqueue(line);
                        }

                        // Advance 2 chars for \r\n
                        lineStart = currentIndex + 2;
                        // Increment one extra plus the end of the loop increment
                        currentIndex++;
                    }
                    else
                    {
                        // Send buffer up to this point, not including \r
                        // But skip if there's no content
                        if (currentIndex == lineStart)
                            break;

                        var line = _sb.ToString(lineStart, currentIndex - lineStart);
                        lock (_messageQueue)
                        {
                            _messageQueue.Enqueue(line);
                        }

                        // Set line start to current index
                        lineStart = currentIndex;
                    }

                    break;
                }
                // Additional handling for Apc escape messages
                case ApcParser.ApcEscape:
                {
                    // Unconditionally consume until StEscape
                    // Look for index of StEscape
                    var searchIndex = currentIndex;
                    while (searchIndex < len && _sb[searchIndex] != ApcParser.StEscape)
                    {
                        searchIndex++;
                    }

                    // If we found StEscape, we have a complete APC message
                    if (searchIndex < len)
                    {
                        // Include the StEscape as part of line.
                        var line = _sb.ToString(lineStart, searchIndex - lineStart + 1);
                        lock (_messageQueue)
                        {
                            _messageQueue.Enqueue(line);
                        }

                        Debug.WriteLine($"AsyncStreamReader - Sent Apc: '{line}'");
                        // Flag to send the next buffer immediately
                        _sendNextBufferImmediately = true;
                        // Advance currentIndex and lineStart to StEscape
                        // lineStart = searchIndex + 1;
                        currentIndex = searchIndex;
                        var remainingStart = currentIndex + 1;
                        var remainingStr = _sb.ToString(
                            remainingStart,
                            _sb.Length - remainingStart
                        );

                        Debug.WriteLine(
                            $"AsyncStreamReader - Sending remaining buffer: '{remainingStr}'"
                        );

                        // Send the rest of the buffer immediately
                        SendRemainingBuffer(currentIndex + 1);
                        return;
                    }

                    // Otherwise continue without any other changes
                    break;
                }
                // If we receive an Ansi escape, send the existing buffer immediately
                // Kind of behaves like newlines
                case '\u001b':
                {
                    // Unlike '\n', this char is not included in the line
                    var line = _sb.ToString(lineStart, currentIndex - lineStart);
                    SendToQueue(line);
                    // Set line start to current index
                    lineStart = currentIndex;

                    // Look ahead and match the escape sequence
                    var remaining = _sb.ToString(currentIndex, len - currentIndex);
                    Match result = AnsiParser.AnsiEscapeSequenceRegex().Match(remaining);

                    // If we found a match, send the escape sequence match, and move forward
                    if (result.Success)
                    {
                        var escapeSequence = result.Value;
                        SendToQueue(escapeSequence);

                        Debug.WriteLine(
                            $"AsyncStreamReader - Sent Ansi escape sequence: {escapeSequence.ToRepr()}"
                        );

                        // Advance currentIndex and lineStart to end of escape sequence
                        // minus 1 since we will increment currentIndex at the end of the loop
                        lineStart = currentIndex + escapeSequence.Length;
                        currentIndex = lineStart - 1;
                    }
                    else
                    {
                        Debug.WriteLine(
                            $"AsyncStreamReader - No match for Ansi escape sequence: {remaining.ToRepr()}"
                        );
                    }

                    break;
                }
            }

            currentIndex++;
        }

        if (len > 0 && _sb[len - 1] == '\r')
        {
            _bLastCarriageReturn = true;
        }

        // If flagged, send remaining buffer immediately
        if (_sendNextBufferImmediately)
        {
            SendRemainingBuffer();
            _sendNextBufferImmediately = false;
            return;
        }

        // Keep the rest characters which can't form a new line in string builder.
        if (lineStart < len)
        {
            if (lineStart == 0)
            {
                // we found no linebreaks, in this case we cache the position
                // so next time we don't have to restart from the beginning
                _currentLinePos = currentIndex;
            }
            else
            {
                _sb.Remove(0, lineStart);
                _currentLinePos = 0;
            }
        }
        else
        {
            _sb.Length = 0;
            _currentLinePos = 0;
        }
    }

    // If everything runs without exception, returns false.
    // If an exception occurs and rethrowInNewThread is true, returns true.
    // If an exception occurs and rethrowInNewThread is false, the exception propagates.
    private bool FlushMessageQueue(bool rethrowInNewThread)
    {
        try
        {
            // Keep going until we're out of data to process.
            while (true)
            {
                // Get the next line (if there isn't one, we're done) and
                // invoke the user's callback with it.
                string? line;
                lock (_messageQueue)
                {
                    if (_messageQueue.Count == 0)
                    {
                        break;
                    }

                    line = _messageQueue.Dequeue();
                }

                if (!_cancelOperation)
                {
                    _userCallBack(line); // invoked outside of the lock
                }
            }

            return false;
        }
        catch (Exception e)
        {
            // If rethrowInNewThread is true, we can't let the exception propagate synchronously on this thread,
            // so propagate it in a thread pool thread and return true to indicate to the caller that this failed.
            // Otherwise, let the exception propagate.
            if (rethrowInNewThread)
            {
                ThreadPool.QueueUserWorkItem(
                    edi => ((ExceptionDispatchInfo)edi!).Throw(),
                    ExceptionDispatchInfo.Capture(e)
                );

                return true;
            }

            throw;
        }
    }
}