using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MuPDFCore.MuPDFRenderer;
using OfficeTools.ViewModels;

namespace OfficeTools.Views;

public partial class PdfPage : UserControl
{
    public PdfPage()
    {
        InitializeComponent();
        Focusable = true;

        DataContext = new PdfPageViewModel(this.FindControl<PDFRenderer>("MuPDFRenderer"));
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        //Render the initial PDF and initialise the PDFRenderer with it.
        (DataContext as PdfPageViewModel)!.VisualOpened();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        (DataContext as PdfPageViewModel)!.VisualClosed();
        base.OnUnloaded(e);
    }

    private void OnKeyDownPageNumberInput(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Focus();
            (DataContext as PdfPageViewModel)!.NaviToView();
        }
    }
}