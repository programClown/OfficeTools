using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OfficeTools.Controls;

public class WordPlainDialogViewModel : ObservableObject
{
    private DateTime? _date;

    private string? _text;

    public WordPlainDialogViewModel()
    {
        Text = "I am PlainDialogViewModel!";
    }

    public DateTime? Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    public string? Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
}