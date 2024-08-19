using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ursa.Controls;

namespace OfficeTools.ViewModels;

public class MainViewViewModel : ViewModelBase
{
    private object? _content;
    private MenuItem? _selectedMenuItem;

    public MenuItem? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            SetProperty(ref _selectedMenuItem, value);
            OnNavigation(value);
        }
    }

    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }


    public ObservableCollection<MenuItem> MenuItems { get; set; } = new()
    {
        new MenuItem
        {
            Header = "Word",
            IconName = "word",
            Children =
            {
                new MenuItem { Header = "doc", IconName = "doc" },
                new MenuItem { Header = "docx", IconName = "docx" }
            }
        },
        new MenuItem
        {
            Header = "Excel",
            IconName = "excel",
            Children =
            {
                new MenuItem { Header = "xls", IconName = "xls" },
                new MenuItem { Header = "xlsx", IconName = "xlsx" }
            }
        },
        new MenuItem { IsSeparator = true },
        new MenuItem { Header = "设置", IconName = "setting" },
        new MenuItem { IsSeparator = true }
    };

    private void OnNavigation(MenuItem? item)
    {
        Content = item!.Header switch
        {
            "doc" => new DocPageViewModel(DocType.Doc),
            "docx" => new DocPageViewModel(DocType.Docx),
            "xls" => new XlsPageViewModel(),
            "xlsx" => new XlsxPageViewModel(),
            "设置" => new SettingsPageViewModel()
        };
    }
}

public class MenuItem
{
    private static Random r = new();

    public MenuItem()
    {
        NavigationCommand = new AsyncRelayCommand(OnNavigate);
    }

    public string? Header { get; set; }
    public string IconName { get; set; }
    public bool IsSeparator { get; set; }
    public ICommand NavigationCommand { get; set; }

    public ObservableCollection<MenuItem> Children { get; set; } = new();

    private async Task OnNavigate()
    {
        await MessageBox.ShowOverlayAsync(Header ?? string.Empty, "Navigation result");
    }

    public IEnumerable<MenuItem> GetLeaves()
    {
        if (Children.Count == 0)
        {
            yield return this;
        }

        foreach (MenuItem child in Children)
        {
            var items = child.GetLeaves();
            foreach (MenuItem item in items)
            {
                yield return item;
            }
        }
    }
}