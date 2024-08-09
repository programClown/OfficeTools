using System;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OfficeTools.Models;

public class WordCoverItem : ObservableObject
{
    private string _company = "汉东省瑞龙科技公司";
    private DateTime _date;
    private string _email = "123@126.com";
    private string _job = "高植物";
    private string _mobile = "12345678901";
    private string _title = "如何向育良书记汇报工作";

    public WordCoverItem()
    {
        Date = DateTime.Today;
    }

    [MinLength(1)]
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Job
    {
        get => _job;
        set => SetProperty(ref _job, value);
    }

    [Length(11, 11)]
    public string Mobile
    {
        get => _mobile;
        set => SetProperty(ref _mobile, value);
    }

    [EmailAddress]
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public DateTime Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    public string Company
    {
        get => _company;
        set => SetProperty(ref _company, value);
    }
}

public class WordFromItem : ObservableObject
{
    private string _firstLevelContent = "这是第一段内容";
    private string _firstLevelFontFamily;
    private int _firstLevelFontSize = 32;
    private string _firstLevelTitle = "第一章";
    private int _id;

    private string _secondLevelContent = "这是第二段内容";
    private string _secondLevelFontFamily;
    private int _secondLevelFontSize = 24;
    private string _secondLevelTitle = "1.1 报告艺术";

    private string _thirdLevelContent = "这是第三段内容";
    private string _thirdLevelFontFamily;
    private int _thirdLevelFontSize = 16;
    private string _thirdLevelTitle = "1.1.1 如何敲门";

    [Range(0, 10000)]
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    [Range(1, 60)]
    public int FirstLevelFontSize
    {
        get => _firstLevelFontSize;
        set => SetProperty(ref _firstLevelFontSize, value);
    }

    [Range(1, 60)]
    public int SecondLevelFontSize
    {
        get => _secondLevelFontSize;
        set => SetProperty(ref _secondLevelFontSize, value);
    }

    [Range(1, 60)]
    public int ThirdLevelFontSize
    {
        get => _thirdLevelFontSize;
        set => SetProperty(ref _thirdLevelFontSize, value);
    }

    [MaxLength(10)]
    public string FirstLevelTitle
    {
        get => _firstLevelTitle;
        set => SetProperty(ref _firstLevelTitle, value);
    }

    public string FirstLevelFontFamily
    {
        get => _firstLevelFontFamily;
        set => SetProperty(ref _firstLevelFontFamily, value);
    }

    public string FirstLevelContent
    {
        get => _firstLevelContent;
        set => SetProperty(ref _firstLevelContent, value);
    }

    [MaxLength(20)]
    public string SecondLevelTitle
    {
        get => _secondLevelTitle;
        set => SetProperty(ref _secondLevelTitle, value);
    }

    public string SecondLevelFontFamily
    {
        get => _secondLevelFontFamily;
        set => SetProperty(ref _secondLevelFontFamily, value);
    }

    public string SecondLevelContent
    {
        get => _secondLevelContent;
        set => SetProperty(ref _secondLevelContent, value);
    }

    [MaxLength(20)]
    public string ThirdLevelTitle
    {
        get => _thirdLevelTitle;
        set => SetProperty(ref _thirdLevelTitle, value);
    }

    public string ThirdLevelFontFamily
    {
        get => _thirdLevelFontFamily;
        set => SetProperty(ref _thirdLevelFontFamily, value);
    }

    public string ThirdLevelContent
    {
        get => _thirdLevelContent;
        set => SetProperty(ref _thirdLevelContent, value);
    }
}