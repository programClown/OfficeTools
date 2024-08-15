using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeTools.Models;
using OfficeTools.ViewModels;

namespace OfficeTools.Controls;

public enum PlainDialogType
{
    WordPlain,
    ImagePlain,
    TablePlain
}

public partial class AddWordImageTableDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap? _imageBitmap;

    [ObservableProperty]
    private string _imageFile;

    [ObservableProperty]
    private bool _isImageAddPlainVisible;

    [ObservableProperty]
    private bool _isTableAddPlainVisible;

    [ObservableProperty]
    private bool _isWordAddPlainVisible;

    [ObservableProperty]
    private string _wordContent;

    public AddWordImageTableDialogViewModel(PlainDialogType plainDialogType)
    {
        SetPlainVisible(plainDialogType);

        SongGridData = new ObservableCollection<SongViewModel>(Song.Songs.Take(3).Select(a => new SongViewModel
                {
                    Title = a.Title,
                    Artist = a.Artist,
                    Album = a.Album,
                    CountOfComment = a.CountOfComment,
                    Duration = a.Duration,
                    NetEaseId = int.Parse(a.Url.Substring(a.Url.LastIndexOf("=") + 1)),
                    IsSelected = false
                }
            )
        );
    }

    public ObservableCollection<SongViewModel> SongGridData { get; set; }

    public void SetPlainVisible(PlainDialogType plainDialogType)
    {
        switch (plainDialogType)
        {
            case PlainDialogType.WordPlain:
                IsWordAddPlainVisible = true;
                IsImageAddPlainVisible = false;
                IsTableAddPlainVisible = false;
                break;

            case PlainDialogType.ImagePlain:
                IsWordAddPlainVisible = false;
                IsImageAddPlainVisible = true;
                IsTableAddPlainVisible = false;
                break;

            case PlainDialogType.TablePlain:
                IsWordAddPlainVisible = false;
                IsImageAddPlainVisible = false;
                IsTableAddPlainVisible = true;
                break;
        }
    }

    [RelayCommand]
    private void AddNewSong()
    {
        SongGridData.Add(new SongViewModel());
    }

    [RelayCommand]
    private async Task OpenImageFile()
    {
        if (App.StorageProvider is null) return;

        var result = await App.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open File",
                FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.ImageAll },
                AllowMultiple = true
            }
        );

        if (result.Count > 0)
        {
            ImageFile = result[0].Path.LocalPath;
            Stream stream = await result[0].OpenReadAsync();
            ImageBitmap = new Bitmap(stream);
        }
    }
}