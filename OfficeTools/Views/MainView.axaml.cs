using Avalonia.Controls;
using OfficeTools.ViewModels;

namespace OfficeTools.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewViewModel();
    }
}