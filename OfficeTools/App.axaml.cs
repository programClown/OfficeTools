using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using OfficeTools.Core.Helper;
using OfficeTools.Core.Services;
using OfficeTools.Extensions;
using OfficeTools.Helper;
using OfficeTools.ViewModels;
using OfficeTools.Views;
using OfficeTools.Views.LogViewer;
using Logger = NLog.Logger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace OfficeTools;

public class App : Application
{
    readonly private static Lazy<Logger> LoggerLazy = new(LogManager.GetCurrentClassLogger);
    private static Logger Logger => LoggerLazy.Value;

    [NotNull]
    public static IServiceProvider? Services { get; internal set; }

    [NotNull]
    public static Visual? VisualRoot { get; internal set; }

    public static IStorageProvider? StorageProvider { get; internal set; }

    public static TopLevel TopLevel => TopLevel.GetTopLevel(VisualRoot)!;

    [NotNull]
    public static IClipboard? Clipboard { get; internal set; }

    // ReSharper disable once MemberCanBePrivate.Global
    [NotNull]
    public static IConfiguration? Config { get; private set; }

    public new static App? Current => (App?)Application.Current;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ConfigureServiceProvider();

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

    internal static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddLazyInstance();

        services.AddSingleton<ISettingsManager, SettingsManager>();

        Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        // if (Compat.IsWindows)
        // {
        //     services.AddSingleton<IPrerequisiteHelper, WindowsPrerequisiteHelper>();
        // }
        // else if (Compat.IsLinux || Compat.IsMacOS)
        // {
        //     services.AddSingleton<IPrerequisiteHelper, UnixPrerequisiteHelper>();
        // }
        //
        // if (!Design.IsDesignMode)
        // {
        //     services.AddSingleton<ILiteDbContext, LiteDbContext>();
        //     services.AddSingleton<IDisposable>(p => p.GetRequiredService<ILiteDbContext>());
        // }

        ConditionalAddLogViewer(services);

        LoggingConfiguration logConfig = ConfigureLogging();
// Add logging
        services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder
                    .AddFilter("Microsoft.Extensions.Http", LogLevel.Warning)
                    .AddFilter("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogLevel.Warning)
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning);

                builder.SetMinimumLevel(LogLevel.Trace);
#if DEBUG
                builder.AddNLog(
                    logConfig,
                    new NLogProviderOptions { IgnoreEmptyEventId = false, CaptureEventId = EventIdCaptureType.Legacy }
                );
#else
            builder.AddNLog(logConfig);
#endif
            }
        );

        return services;
    }

    private static void ConfigureServiceProvider()
    {
        IServiceCollection services = ConfigureServices();

        Services = services.BuildServiceProvider();

        var settingsManager = Services.GetRequiredService<ISettingsManager>();

        string? dataDirectoryOverride = null; //自己设置存储路径
        if (dataDirectoryOverride is not null)
        {
            var normalizedDataDirPath = Path.GetFullPath(dataDirectoryOverride);

            if (Compat.IsWindows)
            {
                // ReSharper disable twice LocalizableElement
                normalizedDataDirPath = normalizedDataDirPath.Replace("\\\\", "\\");
            }

            settingsManager.SetLibraryDirOverride(normalizedDataDirPath);
        }
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
                var result = lifetime.TryShutdown(exitCode);
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

    private static LoggingConfiguration ConfigureLogging()
    {
        ISetupBuilder? setupBuilder = LogManager.Setup();

        ConditionalAddLogViewerNLog(setupBuilder);

        setupBuilder.LoadConfiguration(builder =>
            {
                // Filter some sources to be warn levels or above only
                builder.ForLogger("System.*").WriteToNil(NLog.LogLevel.Warn);
                builder.ForLogger("Microsoft.*").WriteToNil(NLog.LogLevel.Warn);
                builder.ForLogger("Microsoft.Extensions.Http.*").WriteToNil(NLog.LogLevel.Warn);

                // Disable some trace logging by default, unless overriden by app settings
                var typesToDisableTrace = new[] { typeof(ConsoleViewModel) };

                foreach (Type type in typesToDisableTrace)
                {
                    // Skip if app settings already set a level for this type
                    if (
                        Config[$"Logging:LogLevel:{type.FullName}"] is { } levelStr
                        && Enum.TryParse<LogLevel>(levelStr, true, out _)
                    )
                    {
                        continue;
                    }

                    builder.ForLogger(type.FullName).FilterMinLevel(NLog.LogLevel.Debug);
                }

                // Debug console logging
                /*if (Debugger.IsAttached)
                {
                    builder
                        .ForLogger()
                        .FilterMinLevel(NLog.LogLevel.Trace)
                        .WriteTo(
                            new DebuggerTarget("debugger")
                            {
                                Layout = "[${level:uppercase=true}]\t${logger:shortName=true}\t${message}"
                            }
                        )
                        .WithAsync();
                }*/

                // Console logging
                builder
                    .ForLogger()
                    .FilterMinLevel(NLog.LogLevel.Trace)
                    .WriteTo(
                        new ConsoleTarget("console")
                        {
                            Layout = "[${level:uppercase=true}]\t${logger:shortName=true}\t${message}",
                            DetectConsoleAvailable = true
                        }
                    )
                    .WithAsync();

                // File logging
                builder
                    .ForLogger()
                    .FilterMinLevel(NLog.LogLevel.Debug)
                    .WriteTo(
                        new FileTarget("logfile")
                        {
                            Layout =
                                "${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}",
                            FileName = "${specialfolder:folder=ApplicationData}/StabilityMatrix/Logs/app.log",
                            ArchiveOldFileOnStartup = true,
                            ArchiveFileName =
                                "${specialfolder:folder=ApplicationData}/StabilityMatrix/Logs/app.{#}.log",
                            ArchiveDateFormat = "yyyy-MM-dd HH_mm_ss",
                            ArchiveNumbering = ArchiveNumberingMode.Date,
                            MaxArchiveFiles = 9
                        }
                    )
                    .WithAsync();

#if DEBUG
                // LogViewer target when debug mode
                builder
                    .ForLogger()
                    .FilterMinLevel(NLog.LogLevel.Trace)
                    .WriteTo(new DataStoreLoggerTarget { Layout = "${message}" });
#endif
            }
        );

        LogManager.ReconfigExistingLoggers();

        return LogManager.Configuration;
    }

    [Conditional("DEBUG")]
    private static void ConditionalAddLogViewer(IServiceCollection services)
    {
#if DEBUG
        services.AddLogViewer();
#endif
    }

    [Conditional("DEBUG")]
    private static void ConditionalAddLogViewerNLog(ISetupBuilder setupBuilder)
    {
#if DEBUG
        setupBuilder.SetupExtensions(
            extensionBuilder => extensionBuilder.RegisterTarget<DataStoreLoggerTarget>("DataStoreLogger")
        );
#endif
    }
}