using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;
using OfficeTools.Controls;
using OfficeTools.Extensions;
using OfficeTools.Models;
using SkiaSharp;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;

namespace OfficeTools.ViewModels;

public enum DocType
{
    Doc,
    Docx
}

public partial class DocPageViewModel : ViewModelBase
{
    private readonly JsonArray _allWords = new();
    private readonly DocType _docType;

    [ObservableProperty]
    private List<string> _fontFamilyNames = new();

    [ObservableProperty]
    private int _hostControlWidth;

    [ObservableProperty]
    private bool _isOperateEnable = true;

    [ObservableProperty]
    private string _savedDocFile;

    [ObservableProperty]
    private string _suitableWordTitle;

    [ObservableProperty]
    private WordCoverItem _wordCover;

    [ObservableProperty]
    private List<WordFormItem> _wordFromItems;

    public DocPageViewModel(DocType docType)
    {
        _docType = docType;
        SuitableWordTitle = _docType == DocType.Doc ? "适用于Word 2003/2007" : "适用于Word 2013/2016/2019+";

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
        IStorageFile? result;
        result = await App.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save File",
                FileTypeChoices =
                [
                    new FilePickerFileType("Docs") { Patterns = ["*.doc"] },
                    new FilePickerFileType("Docxs") { Patterns = ["*.docx"] }
                ]
            }
        );

        if (result is not null)
        {
            SavedDocFile = result.Path.LocalPath;
            SaveDoc();
        }
    }

    [RelayCommand]
    private void ClearPanel()
    {
        SavedDocFile = "";
        WordCover = new WordCoverItem();
        WordFromItems = new List<WordFormItem> { new() { Id = 0 }, new() { Id = 1 } };
    }

    private void SaveDoc()
    {
        var fs = new FileStream(SavedDocFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        var doc = new XWPFDocument();
        XWPFStyles styles = doc.CreateStyles();
        var sectPr = new CT_SectPr();
        // A4
        sectPr.pgSz.w = 11906;
        sectPr.pgSz.h = 16838;
        doc.Document.body.sectPr = sectPr;

        doc.AddCustomHeadingStyle("标题 1", 1);
        doc.AddCustomHeadingStyle("标题 2", 2);
        doc.AddCustomHeadingStyle("标题 3", 3);

        // 封皮
        XWPFParagraph? paragraph0 = doc.CreateParagraph();
        paragraph0.Alignment = ParagraphAlignment.CENTER; //字体居中 
        paragraph0.SpacingBefore = 400;
        paragraph0.SpacingAfter = 400;
        XWPFRun? run0 = paragraph0.CreateRun();
        run0.FontSize = 48;
        run0.IsBold = true;
        run0.SetFontFamily("黑体", FontCharRange.None);
        run0.SetText("测试报告");
    
        XWPFTable coverTable = doc.CreateTable(6, 2);  
        
        for (var i = 0; i < 6; i++)
        {
            XWPFRun? run01 = paragraph0.CreateRun();
            run01.FontSize = 24;
            run01.IsBold = true;
            run01.SetFontFamily("宋体", FontCharRange.None);
            switch (i)
            {
                case 0:
                    run01.SetText($"项目名：{WordCover.Title}</br>");
                    break;
                case 1:
                    run01.SetText($"职务：{WordCover.Job}</br>");
                    break;
                case 2:
                    run01.SetText($"手机号：{WordCover.Mobile}</br>");
                    break;
                case 3:
                    run01.SetText($"邮箱：{WordCover.Email}</br>");
                    break;
                case 4:
                    run01.SetText($"创建日期：{WordCover.Date}</br>");
                    break;
                case 5:
                    run01.SetText($"单位：{WordCover.Company}</br>");
                    break;
            }
        }


        foreach (WordFormItem wordFormItem in WordFromItems)
        {
            // 第一段
            XWPFParagraph? paragraph1 = doc.CreateParagraph();
            XWPFRun? run1 = paragraph1.CreateRun();
            paragraph1.Style = "标题 1";
            run1.FontSize = wordFormItem.FirstLevelFontSize;
            run1.SetFontFamily(wordFormItem.FirstLevelFontFamily, FontCharRange.None);
            run1.SetText(wordFormItem.FirstLevelTitle);

            XWPFParagraph? paragraph11 = doc.CreateParagraph();
            XWPFRun? run11 = paragraph11.CreateRun();
            run11.FontSize = 12;
            run11.SetFontFamily("宋体", FontCharRange.None);
            run11.SetText(wordFormItem.FirstLevelContent);
            run11.AddBreak();

            // 第二段
            XWPFParagraph? paragraph2 = doc.CreateParagraph();
            XWPFRun? run2 = paragraph2.CreateRun();
            paragraph2.Style = "标题 2";
            run2.FontSize = wordFormItem.SecondLevelFontSize;
            run2.SetFontFamily(wordFormItem.SecondLevelFontFamily, FontCharRange.None);
            run2.SetText(wordFormItem.SecondLevelTitle);

            XWPFParagraph? paragraph21 = doc.CreateParagraph();
            XWPFRun? run21 = paragraph21.CreateRun();
            run21.FontSize = 12;
            run21.SetFontFamily("宋体", FontCharRange.None);
            run21.SetText(wordFormItem.SecondLevelContent);
            run21.AddBreak();

            // 第三段
            XWPFParagraph? paragraph3 = doc.CreateParagraph();
            XWPFRun? run3 = paragraph3.CreateRun();
            paragraph3.Style = "标题 3";
            run3.FontSize = wordFormItem.ThirdLevelFontSize;
            run3.SetFontFamily(wordFormItem.ThirdLevelFontFamily, FontCharRange.None);
            run3.SetText(wordFormItem.ThirdLevelTitle);

            XWPFParagraph? paragraph31 = doc.CreateParagraph();
            XWPFRun? run31 = paragraph31.CreateRun();
            run31.FontSize = 12;
            run31.SetFontFamily("宋体", FontCharRange.None);
            run31.SetText(wordFormItem.ThirdLevelContent);
            run31.AddBreak();
        }

        doc.Write(fs);
        fs.Close();
        doc.Close();
    }

    private void SaveDocx() { }
}