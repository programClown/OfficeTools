using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace OfficeTools.Core.Python.Interop;

/// <summary>
///     Implement the interface of the sys.stdout redirection
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class PyIOStream
{
    readonly private StringBuilder TextBuilder;
    readonly private StringWriter TextWriter;

    public PyIOStream(StringBuilder? builder = null)
    {
        TextBuilder = builder ?? new StringBuilder();
        TextWriter = new StringWriter(TextBuilder);
    }

    public event EventHandler<string>? OnWriteUpdate;

    public void ClearBuffer()
    {
        TextBuilder.Clear();
    }

    public string GetBuffer() => TextBuilder.ToString();

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public void write(string str)
    {
        TextWriter.Write(str);
        OnWriteUpdate?.Invoke(this, str);
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void writelines(IEnumerable<string> str)
    {
        foreach (var line in str)
        {
            write(line);
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void flush()
    {
        TextWriter.Flush();
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void close()
    {
        TextWriter?.Close();
    }
}