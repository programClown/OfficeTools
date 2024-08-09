using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeTools.Controls;
using OfficeTools.Models;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;
using FontFamily = System.Drawing.FontFamily;

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
    private List<WordFromItem> _wordFromItems;

    public DocPageViewModel()
    {
        var fonts = new InstalledFontCollection();
        foreach (FontFamily font in fonts.Families)
        {
            FontFamilyNames.Add(font.Name);
        }

        WordCover = new WordCoverItem();
        WordFromItems = new List<WordFromItem> { new() { Id = 0 }, new() { Id = 1 } };
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
        var vm = new WordPlainDialogViewModel();
        IsOperateEnable = false;
        HostControlWidth = 1600;
        DefaultResult = await Drawer.ShowModal<WordPlainDialog, WordPlainDialogViewModel>(
            vm,
            "LocalHost",
            new DrawerOptions
            {
                Title = "Please select a date",
                Position = Position.Left,
                Buttons = DialogButton.OKCancel,
                CanLightDismiss = true
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
        WordFromItems = new List<WordFromItem> { new() { Id = 0 }, new() { Id = 1 } };
    }
}