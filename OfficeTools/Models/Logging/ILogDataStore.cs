using System.Collections.ObjectModel;

namespace OfficeTools.Models.Logging;

public interface ILogDataStore
{
    ObservableCollection<LogModel> Entries { get; }

    void AddEntry(LogModel logModel);
}