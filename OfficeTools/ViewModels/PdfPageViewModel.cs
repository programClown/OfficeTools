using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuPDFCore;
using MuPDFCore.MuPDFRenderer;
using VectSharp;
using VectSharp.PDF;
using FontFamily = VectSharp.FontFamily;
using Point = VectSharp.Point;

namespace OfficeTools.ViewModels;

public partial class PdfPageViewModel : ViewModelBase
{
    /// <summary>
    ///     An <see cref="EventWaitHandle" /> that signals to the <see cref="UIUpdater" /> thread that the window has been
    ///     closed.
    /// </summary>
    readonly private EventWaitHandle _closedHandle = new(false, EventResetMode.ManualReset);

    readonly private PDFRenderer _pdfRenderer;

    /// <summary>
    ///     A watcher that raises an event if the file that has been opened is overwritten.
    /// </summary>
    readonly private FileSystemWatcher _watcher = new();

    /// <summary>
    ///     The <see cref="MuPDFContext" /> holding the cache and exception stack.
    /// </summary>
    private MuPDFContext _context;

    [ObservableProperty]
    private int _curPageNumber;

    /// <summary>
    ///     The current <see cref="MuPDFDocument" />.
    /// </summary>
    private MuPDFDocument _document;

    [ObservableProperty]
    private int _maxPageNumber;

    private IStorageFile? _selectedFile;

    // We set this to true when the zoom value of the PDF renderer has changed independently of the NumericUpDown (e.g., because the user has used the mouse wheel).
    // In that case, we do not feed back the update to the PDF renderer, to avoid creating a loop.
    private bool _updatingZoomFromRenderer;

    private double _zoomFactor;

    public PdfPageViewModel(PDFRenderer pdfRenderer)
    {
        _pdfRenderer = pdfRenderer;
        _zoomFactor = 1;

        _watcher.Changed += FileChanged;
        _pdfRenderer.PropertyChanged += PdfRendererOnPropertyChanged;
    }

    public async void VisualOpened()
    {
        //Render the initial PDF and initialise the PDFRenderer with it.
        MemoryStream ms = RenderInitialPDF();
        _context = new MuPDFContext();
        _document = new MuPDFDocument(_context, ref ms, InputFileTypes.PDF);

        MaxPageNumber = 0;
        await InitializeDocument(0);

        //Start the UI updater thread.
        // UIUpdater();
    }

    public void VisualClosed()
    {
        //Stop the UI updater thread.
        _closedHandle.Set();

        //Dispose the Document and Context. The PDFRenderer will dispose itself when it detects that it has been detached from the logical tree.
        _document?.Dispose();
        _context?.Dispose();
    }

    /// <summary>
    ///     Initialize the document, showing a progress window for the OCR process, if necessary.
    /// </summary>
    private async Task InitializeDocument(int pageNumber)
    {
        //We need to re-initialise the renderer. No need to ask it to release resources here because it will do it on its own (and we don't need to dispose the Document).
        _pdfRenderer.InitializeAsync(_document, pageNumber: pageNumber, ocrLanguage: null);

        CurPageNumber = _pdfRenderer.PageNumber + 1;

        _updatingZoomFromRenderer = false;
        _zoomFactor = _pdfRenderer.Zoom;
        _updatingZoomFromRenderer = true;
    }

    /// <summary>
    ///     Render the initial PDF document that is shown before a file is opened to a <see cref="MemoryStream" />.
    /// </summary>
    /// <returns></returns>
    private MemoryStream RenderInitialPDF()
    {
        var doc = new Document();

        doc.Pages.Add(new Page(800, 700));

        Graphics gpr = doc.Pages[0].Graphics;

        gpr.Save();
        gpr.Scale(4, 4);
        gpr.Translate(40, 35);

        gpr.FillPath(
            new GraphicsPath().MoveTo(0, 7.5).Arc(7.5, 7.5, 7.5, Math.PI, 2 * Math.PI).LineTo(15, 17.5)
                .Arc(7.5, 17.5, 7.5, 0, Math.PI).Close(),
            Colour.FromCSSString("#e8f0ff").Value
        );

        gpr.FillPath(
            new GraphicsPath().MoveTo(0, 11).LineTo(7.5, 11).LineTo(7.5, 0).Arc(7.5, 7.5, 7.5, -Math.PI / 2, -Math.PI)
                .Close(),
            Colour.FromCSSString("#ff9900").Value
        );

        gpr.StrokePath(
            new GraphicsPath().MoveTo(0, 7.5).Arc(7.5, 7.5, 7.5, Math.PI, 2 * Math.PI).LineTo(15, 17.5)
                .Arc(7.5, 17.5, 7.5, 0, Math.PI).Close(),
            Colour.FromCSSString("#6f8ec6").Value
        );

        gpr.StrokePath(new GraphicsPath().MoveTo(0, 11).LineTo(15, 11).MoveTo(7.5, 0).LineTo(7.5, 11),
            Colour.FromCSSString("#6f8ec6").Value
        );

        gpr.StrokePath(new GraphicsPath().MoveTo(7.5, 4).LineTo(7.5, 8),
            Colour.FromCSSString("#6f8ec6").Value,
            3,
            LineCaps.Round
        );

        gpr.FillPath(
            new GraphicsPath().MoveTo(2.5, -5).LineTo(12.5, -5).LineTo(12.5, -17.5).LineTo(17.5, -17.5)
                .LineTo(7.5, -27.5).LineTo(-2.5, -17.5).LineTo(2.5, -17.5).Close(),
            Colour.FromRgb(180, 180, 180)
        );

        gpr.Save();
        for (var i = 0; i < 3; i++)
        {
            gpr.RotateAt(Math.PI / 2, new Point(7.5, 12.5));
            gpr.FillPath(
                new GraphicsPath().MoveTo(2.5, -5).LineTo(12.5, -5).LineTo(12.5, -17.5).LineTo(17.5, -17.5)
                    .LineTo(7.5, -27.5).LineTo(-2.5, -17.5).LineTo(2.5, -17.5).Close(),
                Colour.FromRgb(180, 180, 180)
            );
        }

        gpr.Restore();

        gpr.Restore();

        gpr.Save();
        gpr.Scale(4, 4);
        gpr.Translate(40, 135);

        gpr.FillPath(
            new GraphicsPath().MoveTo(0, 7.5).Arc(7.5, 7.5, 7.5, Math.PI, 2 * Math.PI).LineTo(15, 17.5)
                .Arc(7.5, 17.5, 7.5, 0, Math.PI).Close(),
            Colour.FromCSSString("#e8f0ff").Value
        );

        gpr.StrokePath(
            new GraphicsPath().MoveTo(0, 7.5).Arc(7.5, 7.5, 7.5, Math.PI, 2 * Math.PI).LineTo(15, 17.5)
                .Arc(7.5, 17.5, 7.5, 0, Math.PI).Close(),
            Colour.FromCSSString("#6f8ec6").Value
        );

        gpr.StrokePath(new GraphicsPath().MoveTo(0, 11).LineTo(15, 11).MoveTo(7.5, 0).LineTo(7.5, 11),
            Colour.FromCSSString("#6f8ec6").Value
        );

        gpr.StrokePath(new GraphicsPath().MoveTo(7.5, 4).LineTo(7.5, 8),
            Colour.FromCSSString("#ff9900").Value,
            3,
            LineCaps.Round
        );

        gpr.Save();
        gpr.Scale(0.5, 0.5);
        gpr.Translate(7.5, 7);
        gpr.FillPath(
            new GraphicsPath().MoveTo(2.5, -5).LineTo(12.5, -5).LineTo(12.5, -17.5).LineTo(17.5, -17.5)
                .LineTo(7.5, -27.5).LineTo(-2.5, -17.5).LineTo(2.5, -17.5).Close(),
            Colour.FromRgb(180, 180, 180)
        );

        gpr.RotateAt(Math.PI, new Point(7.5, 5));
        gpr.FillPath(
            new GraphicsPath().MoveTo(2.5, -5).LineTo(12.5, -5).LineTo(12.5, -17.5).LineTo(17.5, -17.5)
                .LineTo(7.5, -27.5).LineTo(-2.5, -17.5).LineTo(2.5, -17.5).Close(),
            Colour.FromRgb(180, 180, 180)
        );

        gpr.Restore();

        gpr.Restore();

        gpr.FillText(400,
            92,
            "Move the mouse with",
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 35),
            Colours.Gray
        );

        gpr.FillText(400,
            148,
            "the left button pressed",
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 35),
            Colours.Gray
        );

        gpr.FillText(400,
            204,
            "to pan around or to",
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 35),
            Colours.Gray
        );

        gpr.FillText(400,
            260,
            "select text",
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 35),
            Colours.Gray
        );

        gpr.FillText(400,
            530,
            "Use the mouse wheel",
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 35),
            Colours.Gray
        );

        gpr.FillText(400,
            586,
            "to zoom in/out",
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 35),
            Colours.Gray
        );

        var ms = new MemoryStream();
        doc.SaveAsPDF(ms);
        return ms;
    }

    /// <summary>
    ///     Starts a thread that periodically polls the <see cref="DocumentFormat.OpenXml.InkML.Context" /> to determine how
    ///     much memory it is using in the
    ///     asset cache store.
    /// </summary>
    private void UIUpdater()
    {
        var thr = new Thread(async () =>
            {
                //Keep running until the window is closed.
                while (!_closedHandle.WaitOne(0))
                {
                    if (_context != null)
                    {
                        var currentSize = _context.StoreSize;
                        var maxSize = _context.StoreMaxSize;

                        var perc = currentSize / (double)maxSize;
                    }

                    //We don't need to keep polling too often.
                    Thread.Sleep(2000);
                }
            }
        );

        thr.Start();
    }

    private void PdfRendererOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == PDFRenderer.PageNumberProperty)
        {
            CurPageNumber = e.GetNewValue<int>() + 1;
        }
        else if (e.Property == PDFRenderer.ZoomProperty)
        {
            // Prevent the zoom NumericUpDown from re-updating the zoom while we are changing it.
            _updatingZoomFromRenderer = true;
            _zoomFactor = e.GetNewValue<double>();
        }
    }

    private IStorageProvider? GetStorageProvider()
    {
        var topLevel = TopLevel.GetTopLevel(_pdfRenderer);
        return topLevel?.StorageProvider;
    }

    private List<FilePickerFileType>? GetFileTypes() => new() { FilePickerFileTypes.Pdf };

    [RelayCommand]
    private async Task OpenFileClicked()
    {
        IStorageProvider? sp = GetStorageProvider();
        if (sp is null)
        {
            return;
        }

        var files = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "打开文件", FileTypeFilter = GetFileTypes(), AllowMultiple = false
            }
        );

        if (files.Count >= 1)
        {
            _selectedFile = files[0];
            Console.WriteLine(_selectedFile.Name);
            //Now we can dispose the document.
            _document?.Dispose();

            //Create a new document and initialise the PDFRenderer with it.
            _document = new MuPDFDocument(_context, _selectedFile.TryGetLocalPath());

            MaxPageNumber = _document.Pages.Count;
            await InitializeDocument(0);

            //Set up the FileWatcher to keep track of any changes to the file.
            _watcher.EnableRaisingEvents = false;
            _watcher.Path = Path.GetDirectoryName(_selectedFile.TryGetLocalPath());
            _watcher.Filter = Path.GetFileName(_selectedFile.TryGetLocalPath());
            _watcher.EnableRaisingEvents = true;
        }
        else
        {
            _watcher.EnableRaisingEvents = false;
        }
    }

    private void FileChanged(object sender, FileSystemEventArgs e)
    {
        _watcher.EnableRaisingEvents = false;

        Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(1000);

                //Keep track of the current DisplayArea.
                Rect displayArea = _pdfRenderer.DisplayArea;

                //Close the current document and reopen it.
                _pdfRenderer.ReleaseResources();
                _document?.Dispose();
                _document = new MuPDFDocument(_context, e.FullPath);


                MaxPageNumber = _document.Pages.Count;

                await InitializeDocument(0);

                //Restore the DisplayArea.
                _pdfRenderer.SetDisplayAreaNow(displayArea);

                _watcher.EnableRaisingEvents = true;
            }
        );
    }
}