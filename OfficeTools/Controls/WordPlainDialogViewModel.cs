using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeTools.Models;
using OfficeTools.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading.Tasks;
using Ursa.Controls;

namespace OfficeTools.Controls;

public partial class WordPlainDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _selectedId = -1;

    [ObservableProperty]
    private ObservableCollection<WordPlainItem> _wordPlainItems;

    public WordPlainDialogViewModel()
    {
        WordPlainItems = new ObservableCollection<WordPlainItem>();
        TableSongDict = new Dictionary<int, IEnumerable<Song>>();
    }

    public Dictionary<int, IEnumerable<Song>> TableSongDict { get; }

    private int GetMaxId => WordPlainItems.Count == 0 ? 0 : WordPlainItems.Max(s => s.Id);

    [RelayCommand]
    private async Task AddWord()
    {
        var vm = new AddWordImageTableDialogViewModel(PlainDialogType.WordPlain);
        DialogResult result = await Dialog.ShowModal<AddWordImageTableDialog, AddWordImageTableDialogViewModel>(
            vm,
            null,
            new DialogOptions
            {
                Title = "写段文字吧", Mode = DialogMode.Info, Button = DialogButton.YesNo, ShowInTaskBar = false
            }
        );

        if (result == DialogResult.Yes && !string.IsNullOrEmpty(vm.WordContent))
        {
            WordPlainItems.Add(new WordPlainItem { Id = GetMaxId + 1, ContentType = "文字", Content = vm.WordContent });
        }
    }

    [RelayCommand]
    private async Task AddImage()
    {
        var vm = new AddWordImageTableDialogViewModel(PlainDialogType.ImagePlain);
        DialogResult result = await Dialog.ShowModal<AddWordImageTableDialog, AddWordImageTableDialogViewModel>(
            vm,
            null,
            new DialogOptions
            {
                Title = "加个图片吧", Mode = DialogMode.Info, Button = DialogButton.YesNo, ShowInTaskBar = false
            }
        );

        if (result == DialogResult.Yes && !string.IsNullOrEmpty(vm.ImageFile))
        {
            WordPlainItems.Add(new WordPlainItem { Id = GetMaxId + 1, ContentType = "图片", Content = vm.ImageFile });
        }
    }

    [RelayCommand]
    private async Task AddTable()
    {
        var vm = new AddWordImageTableDialogViewModel(PlainDialogType.TablePlain);
        DialogResult result = await Dialog.ShowModal<AddWordImageTableDialog, AddWordImageTableDialogViewModel>(
            vm,
            null,
            new DialogOptions
            {
                Title = "加个表格吧", Mode = DialogMode.Info, Button = DialogButton.YesNo, ShowInTaskBar = false
            }
        );

        if (result == DialogResult.Yes && vm.SongGridData.Count > 0)
        {
            int newId = GetMaxId + 1;
            WordPlainItems.Add(new WordPlainItem
                {
                    Id = newId, ContentType = "表格", Content = $"歌曲{vm.SongGridData.Count}首"
                }
            );

            TableSongDict.Add(newId,
                vm.SongGridData.Select(a =>
                    new Song(a.Title, a.Artist, 0, 0, a.Album, a.CountOfComment, a.NetEaseId) { Duration = a.Duration }
                )
            );

            Console.WriteLine(TableSongDict[newId].Count());
        }
    }

    [RelayCommand]
    private async Task EditItem()
    {
        WordPlainItem plainItem = WordPlainItems[SelectedId];
        switch (plainItem.ContentType)
        {
            case "文字":
                var vm = new AddWordImageTableDialogViewModel(PlainDialogType.WordPlain);
                vm.WordContent = plainItem.Content!;
                DialogResult result = await Dialog.ShowModal<AddWordImageTableDialog, AddWordImageTableDialogViewModel>(
                    vm,
                    null,
                    new DialogOptions
                    {
                        Title = "写段文字吧", Mode = DialogMode.Info, Button = DialogButton.YesNo, ShowInTaskBar = false
                    }
                );

                if (result == DialogResult.Yes && !string.IsNullOrEmpty(vm.WordContent))
                {
                    plainItem.Content = vm.WordContent;
                }

                break;

            case "图片":
                vm = new AddWordImageTableDialogViewModel(PlainDialogType.ImagePlain);
                vm.ImageFile = plainItem.Content!;
                IStorageFile? sf = await App.StorageProvider.TryGetFileFromPathAsync(vm.ImageFile);
                Stream stream = await sf.OpenReadAsync();
                vm.ImageBitmap = new Bitmap(stream);
                result = await Dialog.ShowModal<AddWordImageTableDialog, AddWordImageTableDialogViewModel>(
                    vm,
                    null,
                    new DialogOptions
                    {
                        Title = "加个图片吧", Mode = DialogMode.Info, Button = DialogButton.YesNo, ShowInTaskBar = false
                    }
                );

                if (result == DialogResult.Yes && !string.IsNullOrEmpty(vm.ImageFile))
                {
                    plainItem.Content = vm.ImageFile;
                }

                break;

            case "表格":
                vm = new AddWordImageTableDialogViewModel(PlainDialogType.TablePlain);
                vm.SongGridData = new ObservableCollection<SongViewModel>(
                    TableSongDict.Where(td => td.Key == SelectedId + 1).First().Value.Select(
                        a => new SongViewModel
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

                result = await Dialog.ShowModal<AddWordImageTableDialog, AddWordImageTableDialogViewModel>(
                    vm,
                    null,
                    new DialogOptions
                    {
                        Title = "加个表格吧", Mode = DialogMode.Info, Button = DialogButton.YesNo, ShowInTaskBar = false
                    }
                );

                if (result == DialogResult.Yes && vm.SongGridData.Count > 0)
                {
                    WordPlainItems[SelectedId].Content = $"歌曲{vm.SongGridData.Count}首";

                    TableSongDict[SelectedId + 1] =
                        vm.SongGridData.Select(a =>
                            new Song(a.Title, a.Artist, 0, 0, a.Album, a.CountOfComment, a.NetEaseId)
                            {
                                Duration = a.Duration
                            }
                        );
                }

                break;
        }
    }

    [RelayCommand]
    private void DeleteItem()
    {
        if (WordPlainItems[SelectedId].ContentType == "表格")
        {
            TableSongDict.Remove(SelectedId + 1);
        }

        WordPlainItems.RemoveAt(SelectedId);
    }

    public JsonArray? ToJSON()
    {
        if (WordPlainItems.Count == 0)
        {
            return null;
        }

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), WriteIndented = true
        };

        var plainArray = new JsonArray();
        foreach (WordPlainItem plainItem in WordPlainItems)
        {
            switch (plainItem.ContentType)
            {
                case "文字":
                case "图片":
                    plainArray.Add(plainItem);
                    break;

                case "表格":
                    var tableArray = new JsonArray();
                    foreach (Song song in TableSongDict[plainItem.Id])
                    {
                        tableArray.Add(song);
                    }
                    plainArray.Add(
                        new JsonObject
                        {
                            ["ContentType"] = "表格",
                            ["Content"] = tableArray
                        });
                    break;
            }
        }

        Console.WriteLine(JsonSerializer.Serialize(plainArray, options));

        return plainArray;
    }
}