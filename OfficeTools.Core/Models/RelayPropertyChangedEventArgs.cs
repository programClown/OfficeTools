using System.ComponentModel;

namespace OfficeTools.Core.Models;

public class RelayPropertyChangedEventArgs : PropertyChangedEventArgs
{
    /// <inheritdoc />
    public RelayPropertyChangedEventArgs(string? propertyName, bool isRelay = false)
        : base(propertyName)
    {
        IsRelay = isRelay;
    }

    public bool IsRelay { get; }
}