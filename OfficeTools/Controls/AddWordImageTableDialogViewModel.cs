using System.Collections.ObjectModel;
using System.Linq;
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
}