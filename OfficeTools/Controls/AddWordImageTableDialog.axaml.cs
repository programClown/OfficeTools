using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace OfficeTools.Controls;

public partial class AddWordImageTableDialog : UserControl
{
    public AddWordImageTableDialog()
    {
        InitializeComponent();
    }

    private async void OpenImageFile(object? sender, RoutedEventArgs e)
    {
        IStorageProvider? sp = GetStorageProvider();
        if (sp is null) return;
        var result = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open File",
                FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.ImageAll },
                AllowMultiple = true
            }
        );

        if (result.Count > 0)
        {
            (DataContext as AddWordImageTableDialogViewModel).ImageFile = result[0].Path.LocalPath;

            Stream stream = await result[0].OpenReadAsync();
            imageViewer.Source = new Bitmap(stream);
        }
    }

    private IStorageProvider? GetStorageProvider()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel?.StorageProvider;
    }
}