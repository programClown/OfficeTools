using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using MuPDFCore.MuPDFRenderer;

namespace OfficeTools.Views;

public partial class PdfPage : UserControl
{
    public PdfPage()
    {
        InitializeComponent();
    }

    private void RendererPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == PDFRenderer.DisplayAreaProperty)
        {
            var renderer = this.FindControl<PDFRenderer>("MuPDFRenderer");

            Rect displayArea = renderer.DisplayArea;
            Rect pageSize = renderer.PageSize;

            var minX = Math.Min(displayArea.Left, pageSize.Left);
            var minY = Math.Min(displayArea.Top, pageSize.Top);
            var maxX = Math.Max(displayArea.Right, pageSize.Right);
            var maxY = Math.Max(displayArea.Bottom, pageSize.Bottom);

            var width = maxX - minX;
            var height = maxY - minY;

            var size = Math.Max(width, height);

            minX -= (size - width) * 0.5;
            maxX += (size - width) * 0.5;
            minY -= (size - height) * 0.5;
            maxY += (size - height) * 0.5;

            var pageRect = this.FindControl<Image>("PageAreaImage");
            var pageCanavs = this.FindControl<Canvas>("PageAreaCanvas");
            var displayRect = this.FindControl<Rectangle>("DisplayAreaRectangle");

            pageRect.Width = pageSize.Width / (maxX - minX) * 200;
            pageRect.Height = pageSize.Height / (maxY - minY) * 200;

            pageCanavs.Width = pageSize.Width / (maxX - minX) * 200;
            pageCanavs.Height = pageSize.Height / (maxY - minY) * 200;
            pageCanavs.RenderTransform = new TranslateTransform((pageSize.Left - minX) / (maxX - minX) * 200,
                (pageSize.Top - minY) / (maxY - minY) * 200
            );

            displayRect.Width = displayArea.Width / (maxX - minX) * 200;
            displayRect.Height = displayArea.Height / (maxY - minY) * 200;
            displayRect.RenderTransform = new TranslateTransform((displayArea.Left - minX) / (maxX - minX) * 200,
                (displayArea.Top - minY) / (maxY - minY) * 200
            );
        }
    }
}