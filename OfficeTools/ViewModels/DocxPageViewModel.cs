using System.Collections.ObjectModel;
using System.Linq;

namespace OfficeTools.ViewModels;

public class DocxPageViewModel : ViewModelBase
{
    public ObservableCollection<string> ListStringItems { get; set; } =
        new(Enumerable.Range(0, 1000).Select(a => "Item " + a));
}