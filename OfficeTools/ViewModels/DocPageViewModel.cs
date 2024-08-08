using System.Collections.ObjectModel;
using System.Linq;
using OfficeTools.Models;

namespace OfficeTools.ViewModels;

public class DocPageViewModel : ViewModelBase
{
    public ObservableCollection<ButtonItem> SwitchButtonItems { get; set; } = new()
    {
        new ButtonItem { Name = "Edit", Tips = "编辑" },
        new ButtonItem { Name = "ImageEdit", Tips = "编辑+预览" },
        new ButtonItem { Name = "Image", Tips = "预览" }
    };

    public ObservableCollection<string> ListStringItems { get; set; } =
        new(Enumerable.Range(0, 50).Select(a => "Item " + a));
}