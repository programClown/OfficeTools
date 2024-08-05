using Ursa.Controls;
using WebViewControl;

namespace OfficeTools.Views;

public partial class MainWindow : UrsaWindow
{
    public MainWindow()
    {
        WebView.Settings.OsrEnabled = false;
        WebView.Settings.LogFile = "ceflog.log";

        InitializeComponent();
    }
}