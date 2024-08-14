using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using OfficeTools.ViewModels;
using OfficeTools.Views;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace OfficeTools;

public class App : Application
{
    [NotNull]
    public static IServiceProvider? Services { get; }

    [NotNull]
    public static Visual? VisualRoot { get; internal set; }

    public static IStorageProvider? StorageProvider { get; internal set; }

    public static TopLevel TopLevel => TopLevel.GetTopLevel(VisualRoot)!;

    [NotNull]
    public static IClipboard? Clipboard { get; internal set; }

    public new static App? Current => (App?)Application.Current;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            //BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };

            VisualRoot = desktop.MainWindow;
            StorageProvider = desktop.MainWindow.StorageProvider;
            Clipboard = desktop.MainWindow.Clipboard ?? throw new NullReferenceException("Clipboard is null");

            desktop.Exit += OnApplicationLifetimeExit;
            desktop.ShutdownRequested += OnShutdownRequested;

            AppDomain.CurrentDomain.ProcessExit += OnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, EventArgs e)
    {
    }

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
    }

    private void OnApplicationLifetimeExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        OnExit(sender, e);
    }

    public static void Shutdown(int exitCode = 0)
    {
        if (Current is null)
        {
            throw new NullReferenceException("Current Application was null when Shutdown called");
        }

        if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            try
            {
                bool result = lifetime.TryShutdown(exitCode);
                Debug.WriteLine($"Shutdown: {result}");

                if (result)
                {
                    Environment.Exit(exitCode);
                }
            }
            catch (InvalidOperationException)
            {
                // Ignore in case already shutting down
            }
        }
        else
        {
            Environment.Exit(exitCode);
        }
    }
}