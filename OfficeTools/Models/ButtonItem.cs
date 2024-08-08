using System.Windows.Input;

namespace OfficeTools.Models;

public class ButtonItem
{
    public string? Name { get; set; }
    public string? Tips { get; set; }
    public ICommand? InvokeCommand { get; set; }
}