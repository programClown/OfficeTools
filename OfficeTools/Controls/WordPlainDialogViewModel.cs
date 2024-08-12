using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeTools.Models;
using OfficeTools.ViewModels;
using Ursa.Controls;

namespace OfficeTools.Controls;

public partial class WordPlainDialogViewModel : ViewModelBase
{
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
    }
}