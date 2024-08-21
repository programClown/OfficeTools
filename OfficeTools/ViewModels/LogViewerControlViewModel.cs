using OfficeTools.Models.Logging;

namespace OfficeTools.ViewModels;

public class LogViewerControlViewModel : ViewModelBase, ILogDataStoreImpl
{
    #region Constructor

    public LogViewerControlViewModel(ILogDataStore dataStore)
    {
        DataStore = dataStore;
    }

    #endregion

    #region Properties

    public ILogDataStore DataStore { get; set; }

    #endregion
}