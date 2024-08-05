using Avalonia.Controls;
using WebViewControl;

namespace OfficeTools.Views;

public partial class DocPage : UserControl
{
    public DocPage()
    {
        WebView.Settings.OsrEnabled = false;
        WebView.Settings.LogFile = "ceflog.txt";
        InitializeComponent();
    }
}