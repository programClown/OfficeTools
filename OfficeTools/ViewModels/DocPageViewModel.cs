using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeTools.Controls;
using OfficeTools.Models;
using SkiaSharp;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;

namespace OfficeTools.ViewModels;

public partial class DocPageViewModel : ViewModelBase
{
    readonly private JsonArray _allWords = new();

    [ObservableProperty]
    private List<string> _fontFamilyNames = new();

    [ObservableProperty]
    private int _hostControlWidth;

    [ObservableProperty]
    private bool _isOperateEnable = true;

    [ObservableProperty]
    private string _savedDocFile;

    [ObservableProperty]
    private WordCoverItem _wordCover;

    [ObservableProperty]
    private List<WordFormItem> _wordFromItems;

    public DocPageViewModel()
    {
        // foreach (var fontFamily in FontFamily.StandardFamilies)
        // {
        //     FontFamilyNames.Add(fontFamily);
        // }

        var ft = SKFontManager.Default.FontFamilyCount;
        for (var i = 0; i < SKFontManager.Default.FontFamilyCount; i++)
        {
            FontFamilyNames.Add(SKFontManager.Default.GetFamilyName(i));
        }

        if (!FontFamilyNames.Contains("黑体"))
        {
            FontFamilyNames.Add("黑体");
        }

        if (!FontFamilyNames.Contains("微软雅黑"))
        {
            FontFamilyNames.Add("微软雅黑");
        }

        if (!FontFamilyNames.Contains("宋体"))
        {
            FontFamilyNames.Add("宋体");
        }

        WordCover = new WordCoverItem();
        WordFromItems = new List<WordFormItem> { new() { Id = 0 }, new() { Id = 1 } };
    }

    private JsonSerializerOptions JsonOptions => new() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

    public ObservableCollection<ButtonItem> SwitchButtonItems { get; set; } = new()
    {
        new ButtonItem { Name = "Edit", Tips = "编辑" },
        new ButtonItem { Name = "ImageEdit", Tips = "编辑+预览" },
        new ButtonItem { Name = "Image", Tips = "预览" }
    };

    public ObservableCollection<string> ListStringItems { get; set; } =
        new(Enumerable.Range(0, 50).Select(a => "Item " + a));

    [RelayCommand]
    private async Task AddFisrtLevel(int id)
    {
        var vm = new WordPlainDialogViewModel();
        IsOperateEnable = false;
        HostControlWidth = 1600;
        DialogResult result = await Drawer.ShowModal<WordPlainDialog, WordPlainDialogViewModel>(
            vm,
            "LocalHost",
            new DrawerOptions
            {
                Title = "请给doc文件添加内容",
                Position = Position.Left,
                Buttons = DialogButton.OKCancel,
                CanLightDismiss = false,
                MinWidth = 600
            }
        );

        JsonArray? json = vm.ToJSON();
        _allWords.Add(json);
        WordFromItems[id].FirstLevelContent = JsonSerializer.Serialize(json, JsonOptions);
        HostControlWidth = 0;
        IsOperateEnable = true;
    }

    [RelayCommand]
    private async Task AddSecondLevel(int id)
    {
        var vm = new WordPlainDialogViewModel();
        IsOperateEnable = false;
        HostControlWidth = 1600;
        DialogResult result = await Drawer.ShowModal<WordPlainDialog, WordPlainDialogViewModel>(
            vm,
            "LocalHost",
            new DrawerOptions
            {
                Title = "请给doc文件添加内容",
                Position = Position.Left,
                Buttons = DialogButton.OKCancel,
                CanLightDismiss = false,
                MinWidth = 600
            }
        );

        JsonArray? json = vm.ToJSON();
        _allWords.Add(json);
        HostControlWidth = 0;
        WordFromItems[id].SecondLevelContent = JsonSerializer.Serialize(json, JsonOptions);
        IsOperateEnable = true;
    }

    [RelayCommand]
    private async Task AddThirdLevel(int id)
    {
        var vm = new WordPlainDialogViewModel();
        IsOperateEnable = false;
        HostControlWidth = 1600;
        DialogResult result = await Drawer.ShowModal<WordPlainDialog, WordPlainDialogViewModel>(
            vm,
            "LocalHost",
            new DrawerOptions
            {
                Title = "请给doc文件添加内容",
                Position = Position.Left,
                Buttons = DialogButton.OKCancel,
                CanLightDismiss = false,
                MinWidth = 600
            }
        );

        JsonArray? json = vm.ToJSON();
        _allWords.Add(json);
        WordFromItems[id].ThirdLevelContent = JsonSerializer.Serialize(json, JsonOptions);
        HostControlWidth = 0;
        IsOperateEnable = true;
    }

    [RelayCommand]
    private async Task SaveFile()
    {
        IStorageFile? result =
            await App.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = "Open File",
                    FileTypeChoices =
                        new List<FilePickerFileType> { new("") { Patterns = new[] { "*.doc" } } }
                }
            );

        if (result is not null)
        {
            SavedDocFile = result.Path.LocalPath;
        }
    }

    [RelayCommand]
    private void ClearPanel()
    {
        SavedDocFile = "";
        WordCover = new WordCoverItem();
        WordFromItems = new List<WordFormItem> { new() { Id = 0 }, new() { Id = 1 } };
    }
}