﻿using Octokit;
using OfficeTools.Core.Helper;
using OfficeTools.Core.Helper.HardwareInfo;
using OfficeTools.Core.Models;
using OfficeTools.Core.Models.Database;
using OfficeTools.Core.Models.FileInterfaces;
using OfficeTools.Core.Models.Packages;
using OfficeTools.Core.Models.Progress;
using OfficeTools.Core.Processes;
using OfficeTools.Core.Python;
using PackageType = OfficeTools.Core.Models.PackageType;

namespace OfficeTools.Core.Packages;

public abstract class BasePackage
{
    public string ByAuthor => $"By {Author}";

    public abstract string Name { get; }
    public abstract string DisplayName { get; set; }
    public abstract string Author { get; }
    public abstract string Blurb { get; }
    public abstract string GithubUrl { get; }
    public abstract string LicenseType { get; }
    public abstract string LicenseUrl { get; }
    public virtual string Disclaimer => string.Empty;
    public virtual bool OfferInOneClickInstaller => true;

    /// <summary>
    ///     Primary command to launch the package. 'Launch' buttons uses this.
    /// </summary>
    public abstract string LaunchCommand { get; }

    /// <summary>
    ///     Optional commands (e.g. 'config') that are on the launch button split drop-down.
    /// </summary>
    public virtual IReadOnlyList<string> ExtraLaunchCommands { get; } = Array.Empty<string>();

    public abstract Uri PreviewImageUri { get; }
    public virtual bool ShouldIgnoreReleases => false;
    public virtual bool UpdateAvailable { get; set; }

    public virtual bool IsInferenceCompatible => false;

    public abstract string OutputFolderName { get; }

    public abstract IEnumerable<TorchVersion> AvailableTorchVersions { get; }

    public virtual bool IsCompatible => GetRecommendedTorchVersion() != TorchVersion.Cpu;

    public abstract PackageDifficulty InstallerSortOrder { get; }

    public virtual PackageType PackageType => PackageType.SdInference;

    public virtual IEnumerable<SharedFolderMethod> AvailableSharedFolderMethods => new[]
    {
        SharedFolderMethod.Symlink, SharedFolderMethod.Configuration, SharedFolderMethod.None
    };

    public abstract SharedFolderMethod RecommendedSharedFolderMethod { get; }

    public abstract List<LaunchOptionDefinition> LaunchOptions { get; }
    public virtual string? ExtraLaunchArguments { get; set; } = null;

    /// <summary>
    ///     The shared folders that this package supports.
    ///     Mapping of <see cref="SharedFolderType" /> to the relative paths from the package root.
    /// </summary>
    public abstract Dictionary<SharedFolderType, IReadOnlyList<string>>? SharedFolders { get; }

    public abstract Dictionary<SharedOutputType, IReadOnlyList<string>>? SharedOutputFolders { get; }

    /// <summary>
    ///     If defined, this package supports extensions using this manager.
    /// </summary>
    // public virtual IPackageExtensionManager? ExtensionManager => null;

    /// <summary>
    ///     True if this package supports extensions.
    /// </summary>
    // [MemberNotNullWhen(true, nameof(ExtensionManager))]
    // public virtual bool SupportsExtensions => ExtensionManager is not null;

    public abstract string MainBranch { get; }

    public virtual PackageVersionType AvailableVersionTypes => ShouldIgnoreReleases
        ? PackageVersionType.Commit
        : PackageVersionType.GithubRelease | PackageVersionType.Commit;

    public virtual IEnumerable<PackagePrerequisite> Prerequisites =>
    [
        PackagePrerequisite.Git, PackagePrerequisite.Python310, PackagePrerequisite.VcRedist
    ];

    public abstract Task DownloadPackage(
        string installLocation,
        DownloadPackageVersionOptions versionOptions,
        IProgress<ProgressReport>? progress1
    );

    public abstract Task InstallPackage(
        string installLocation,
        TorchVersion torchVersion,
        SharedFolderMethod selectedSharedFolderMethod,
        DownloadPackageVersionOptions versionOptions,
        IProgress<ProgressReport>? progress = null,
        Action<ProcessOutput>? onConsoleOutput = null
    );

    public abstract Task RunPackage(
        string installedPackagePath,
        string command,
        string arguments,
        Action<ProcessOutput>? onConsoleOutput
    );

    public abstract Task<bool> CheckForUpdates(InstalledPackage package);

    public abstract Task<InstalledPackageVersion> Update(
        InstalledPackage installedPackage,
        TorchVersion torchVersion,
        DownloadPackageVersionOptions versionOptions,
        IProgress<ProgressReport>? progress = null,
        bool includePrerelease = false,
        Action<ProcessOutput>? onConsoleOutput = null
    );

    public abstract Task SetupModelFolders(
        DirectoryPath installDirectory,
        SharedFolderMethod sharedFolderMethod
    );

    public abstract Task UpdateModelFolders(
        DirectoryPath installDirectory,
        SharedFolderMethod sharedFolderMethod
    );

    public abstract Task RemoveModelFolderLinks(
        DirectoryPath installDirectory,
        SharedFolderMethod sharedFolderMethod
    );

    public abstract Task SetupOutputFolderLinks(DirectoryPath installDirectory);

    public abstract Task RemoveOutputFolderLinks(DirectoryPath installDirectory);

    public virtual TorchVersion GetRecommendedTorchVersion()
    {
        // if there's only one AvailableTorchVersion, return that
        if (AvailableTorchVersions.Count() == 1)
        {
            return AvailableTorchVersions.First();
        }

        if (HardwareHelper.HasNvidiaGpu() && AvailableTorchVersions.Contains(TorchVersion.Cuda))
        {
            return TorchVersion.Cuda;
        }

        if (HardwareHelper.PreferRocm() && AvailableTorchVersions.Contains(TorchVersion.Rocm))
        {
            return TorchVersion.Rocm;
        }

        if (HardwareHelper.PreferDirectML() && AvailableTorchVersions.Contains(TorchVersion.DirectMl))
        {
            return TorchVersion.DirectMl;
        }

        if (Compat.IsMacOS && Compat.IsArm && AvailableTorchVersions.Contains(TorchVersion.Mps))
        {
            return TorchVersion.Mps;
        }

        return TorchVersion.Cpu;
    }

    /// <summary>
    ///     Shuts down the subprocess, canceling any pending streams.
    /// </summary>
    public abstract void Shutdown();

    /// <summary>
    ///     Shuts down the process, returning a Task to wait for output EOF.
    /// </summary>
    public abstract Task WaitForShutdown();

    public abstract Task<IEnumerable<Release>> GetReleaseTags();

    public abstract Task<PackageVersionOptions> GetAllVersionOptions();

    public abstract Task<IEnumerable<GitCommit>?> GetAllCommits(
        string branch,
        int page = 1,
        int perPage = 10
    );

    public abstract Task<DownloadPackageVersionOptions> GetLatestVersion(bool includePrerelease = false);

    public event EventHandler<int>? Exited;
    public event EventHandler<string>? StartupComplete;

    public void OnExit(int exitCode) => Exited?.Invoke(this, exitCode);

    public void OnStartupComplete(string url) => StartupComplete?.Invoke(this, url);

    protected async Task InstallCudaTorch(
        PyVenvRunner venvRunner,
        IProgress<ProgressReport>? progress = null,
        Action<ProcessOutput>? onConsoleOutput = null
    )
    {
        progress?.Report(new ProgressReport(-1f, "Installing PyTorch for CUDA", isIndeterminate: true));

        await venvRunner
            .PipInstall(
                new PipInstallArgs()
                    .WithTorch("==2.1.2")
                    .WithTorchVision("==0.16.2")
                    .WithXFormers("==0.0.23post1")
                    .WithTorchExtraIndex("cu121"),
                onConsoleOutput
            )
            .ConfigureAwait(false);
    }

    protected Task InstallDirectMlTorch(
        PyVenvRunner venvRunner,
        IProgress<ProgressReport>? progress = null,
        Action<ProcessOutput>? onConsoleOutput = null
    )
    {
        progress?.Report(new ProgressReport(-1f, "Installing PyTorch for DirectML", isIndeterminate: true));

        return venvRunner.PipInstall(new PipInstallArgs().WithTorchDirectML(), onConsoleOutput);
    }

    protected Task InstallCpuTorch(
        PyVenvRunner venvRunner,
        IProgress<ProgressReport>? progress = null,
        Action<ProcessOutput>? onConsoleOutput = null
    )
    {
        progress?.Report(new ProgressReport(-1f, "Installing PyTorch for CPU", isIndeterminate: true));

        return venvRunner.PipInstall(
            new PipInstallArgs().WithTorch("==2.1.2").WithTorchVision(),
            onConsoleOutput
        );
    }

    public abstract Task<DownloadPackageVersionOptions?> GetUpdate(InstalledPackage installedPackage);
}