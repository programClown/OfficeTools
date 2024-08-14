using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeTools.Controls;
using OfficeTools.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;

namespace OfficeTools.ViewModels;

public partial class DocPageViewModel : ViewModelBase
{
    [ObservableProperty] private DialogResult? _defaultResult;

    [ObservableProperty]
    private List<string> _fontFamilyNames = new();


    [ObservableProperty]
    private int _hostControlWidth;

    [ObservableProperty]
    private bool _isOperateEnable = true;

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

        int ft = SKFontManager.Default.FontFamilyCount;
        for (int i = 0; i < SKFontManager.Default.FontFamilyCount; i++)
        {
            FontFamilyNames.Add(SKFontManager.Default.GetFamilyName(i));
        }

        WordCover = new WordCoverItem();
        WordFromItems = new List<WordFormItem> { new() { Id = 0 }, new() { Id = 1 } };
    }

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
        throw new Exception("test");
        var vm = new WordPlainDialogViewModel();
        IsOperateEnable = false;
        HostControlWidth = 1600;
        DefaultResult = await Drawer.ShowModal<WordPlainDialog, WordPlainDialogViewModel>(
            vm,
            "LocalHost",
            new DrawerOptions
            {
                Title = "请给doc文件添加内容",
                Position = Position.Left,
                Buttons = DialogButton.OKCancel,
                CanLightDismiss = true,
                MinWidth = 600
            }
        );

        HostControlWidth = 0;
        IsOperateEnable = true;
    }

    [RelayCommand]
    private void AddSecondLevel(int id)
    {
        Console.WriteLine($"二级{id}");
    }

    [RelayCommand]
    private void AddThirdLevel(int id)
    {
        Console.WriteLine($"三级{id}");
    }

    [RelayCommand]
    private void SaveFile()
    {
        Console.WriteLine(WordFromItems.Count);
    }

    [RelayCommand]
    private void ClearPanel()
    {
        WordCover = new WordCoverItem();
        WordFromItems = new List<WordFormItem> { new() { Id = 0 }, new() { Id = 1 } };
    }
}