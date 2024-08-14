using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeTools.Models;
using OfficeTools.ViewModels;
using System.Collections.ObjectModel;
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
    }

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
            WordPlainItems.Add(new WordPlainItem
            {
                Id = GetMaxId(),
                ContentType = "文字",
                Content = vm.WordContent
            });
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
            WordPlainItems.Add(new WordPlainItem
            {
                Id = GetMaxId(),
                ContentType = "图片",
                Content = vm.ImageFile
            });
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
            WordPlainItems.Add(new WordPlainItem
            {
                Id = GetMaxId(),
                ContentType = "表格",
                Content = $"歌曲{vm.SongGridData.Count}首"
            });
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
                vm.WordContent = plainItem.Content.ToString()!;
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
                vm.ImageFile = plainItem.Content.ToString()!;
                result = await Dialog.ShowModal<AddWordImageTableDialog, AddWordImageTableDialogViewModel>(
                    vm,
                    null,
                    new DialogOptions
                    {
                        Title = "加个图片吧", Mode = DialogMode.Info, Button = DialogButton.YesNo, ShowInTaskBar = false
                    }
                );

                if (result == DialogResult.Yes && string.IsNullOrEmpty(vm.ImageFile))
                {
                    plainItem.Content = vm.ImageFile;
                }
                break;

            case "表格":
                MessageBox.ShowAsync("警告", "现在还未做");
                break;
        }
    }

    [RelayCommand]
    private void DeleteItem()
    {
        WordPlainItems.RemoveAt(SelectedId);
    }

    private int GetMaxId()
    {
        int maxId = 0;
        foreach (WordPlainItem plainItem in WordPlainItems)
        {
            if (maxId < plainItem.Id)
            {
                maxId = plainItem.Id;
            }
        }

        return maxId + 1;
    }
}