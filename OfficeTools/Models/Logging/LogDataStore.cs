using System.Collections.ObjectModel;
using System.Threading;
using Avalonia.Threading;

namespace OfficeTools.Models.Logging;

public class LogDataStore : ILogDataStore
{
    #region Fields

    readonly private static SemaphoreSlim _semaphore = new(1);

    #endregion

    public static LogDataStore Instance { get; } = new();

    #region Properties

    public ObservableCollection<LogModel> Entries { get; } = new();

    #endregion

    #region Methods

    public virtual void AddEntry(LogModel logModel)
    {
        // ensure only one operation at time from multiple threads
        _semaphore.Wait();

        Dispatcher.UIThread.Post(() => { Entries.Add(logModel); });

        _semaphore.Release();
    }

    #endregion
}