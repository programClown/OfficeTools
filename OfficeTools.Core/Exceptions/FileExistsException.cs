namespace OfficeTools.Core.Exceptions;

public class FileTransferExistsException : IOException
{
    public FileTransferExistsException(string source, string destination)
    {
        SourceFile = source;
        DestinationFile = destination;
    }

    public string SourceFile { get; }
    public string DestinationFile { get; }
}