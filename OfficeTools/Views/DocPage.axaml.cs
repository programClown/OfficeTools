using Avalonia.Controls;
using Avalonia.Interactivity;

namespace OfficeTools.Views;

public partial class DocPage : UserControl
{
    public DocPage()
    {
        InitializeComponent();
        Focusable = true;
    }

    private void LeftListBoxOnLostFocus(object? sender, RoutedEventArgs e)
    {
        leftListBox.SelectedIndex = -1;
    }
}