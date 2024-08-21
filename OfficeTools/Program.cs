using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using NLog;
using OfficeTools.Controls;
using OfficeTools.Core.Helper;
using Semver;
using Ursa.Controls;

namespace OfficeTools;

public static class Program
{
    private static Logger? _logger;
    private static Logger Logger => _logger ??= LogManager.GetCurrentClassLogger();

    public static Stopwatch StartupTimer { get; } = new();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        StartupTimer.Start();

        var infoVersion = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        Compat.AppVersion = SemVersion.Parse(infoVersion ?? "0.0.0", SemVersionStyles.Strict);

        // Configure exception dialog for unhandled exceptions
        if (!Debugger.IsAttached)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            // .UseManagedSystemDialogs()
            .UsePlatformDetect()
            .UseAliBabaFontFamily()
            .LogToTrace();

    /// <summary>
    ///     Wait for an external process to exit,
    ///     ignores if process is not found, already exited, or throws an exception
    /// </summary>
    /// <param name="pid">External process PID</param>
    /// <param name="timeout">Timeout to wait for process to exit</param>
    public static void WaitForPidExit(int pid, TimeSpan timeout)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            process.WaitForExitAsync(new CancellationTokenSource(timeout).Token).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            Logger.Warn("Timed out ({Timeout:g}) waiting for process {Pid} to exit", timeout, pid);
        }
        catch (SystemException e)
        {
            Logger.Warn(e, "Failed to wait for process {Pid} to exit", pid);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Unexpected error during WaitForPidExit");
            throw;
        }
    }


    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception ex)
        {
            return;
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            var dialogViewModel = new ExceptionDialogViewModel { Exception = ex };

            // Window? mainWindow = lifetime.MainWindow;
            // // We can only show dialog if main window exists, and is visible
            // if (mainWindow is { PlatformImpl: not null, IsVisible: true })
            // {
            // }

            var cts = new CancellationTokenSource();
            Dialog.ShowModal<ExceptionDialog, ExceptionDialogViewModel>(
                dialogViewModel,
                options: new DialogOptions
                {
                    Title = "异常问题", Mode = DialogMode.Error, Button = DialogButton.None, ShowInTaskBar = false
                }
            ).WaitAsync(cts.Token).ContinueWith(
                _ =>
                {
                    cts.Cancel();
                    ExitWithException(ex);
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );

            Dispatcher.UIThread.MainLoop(cts.Token);
        }
    }

    [DoesNotReturn]
    private static void ExitWithException(Exception exception)
    {
        App.Shutdown(1);
        Dispatcher.UIThread.InvokeShutdown();
        Environment.Exit(Marshal.GetHRForException(exception));
    }
}