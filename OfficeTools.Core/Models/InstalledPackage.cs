﻿using System.Text.Json.Serialization;
using OfficeTools.Core.Helper;

namespace OfficeTools.Core.Models;

/// <summary>
///     Profile information for a user-installed package.
/// </summary>
public class InstalledPackage : IJsonOnDeserialized
{
    // Unique ID for the installation
    public Guid Id { get; set; }

    // User defined name
    public string? DisplayName { get; set; }

    // Package name
    public string? PackageName { get; set; }

    // Package version
    [Obsolete("Use Version instead. (Kept for migration)")]
    public string? PackageVersion { get; set; }

    [Obsolete("Use Version instead. (Kept for migration)")]
    public string? InstalledBranch { get; set; }

    [Obsolete("Use Version instead. (Kept for migration)")]
    public string? DisplayVersion { get; set; }

    public InstalledPackageVersion? Version { get; set; }

    // Old type absolute path
    [Obsolete("Use LibraryPath instead. (Kept for migration)")]
    public string? Path { get; set; }

    /// <summary>
    ///     Relative path from the library root.
    /// </summary>
    public string? LibraryPath { get; set; }

    /// <summary>
    ///     Full path to the package, using LibraryPath and GlobalConfig.LibraryDir.
    /// </summary>
    [JsonIgnore]
    public string? FullPath => LibraryPath != null ? System.IO.Path.Combine(GlobalConfig.LibraryDir, LibraryPath) : null;

    public string? LaunchCommand { get; set; }
    public List<LaunchOption>? LaunchArgs { get; set; }
    public DateTimeOffset? LastUpdateCheck { get; set; }
    public bool UpdateAvailable { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter<TorchVersion>))]
    public TorchVersion? PreferredTorchVersion { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter<SharedFolderMethod>))]
    public SharedFolderMethod? PreferredSharedFolderMethod { get; set; }

    public bool UseSharedOutputFolder { get; set; }

    public List<string>? ExtraExtensionManifestUrls { get; set; }

    public static IEqualityComparer<InstalledPackage> Comparer { get; } =
        new PropertyComparer<InstalledPackage>(p => p.Id);

#pragma warning disable CS0618 // Type or member is obsolete
    public void OnDeserialized()
    {
        // Handle version migration
        if (Version != null)
            return;

        if (string.IsNullOrWhiteSpace(InstalledBranch) && !string.IsNullOrWhiteSpace(PackageVersion))
        {
            // release mode
            Version = new InstalledPackageVersion { InstalledReleaseVersion = PackageVersion, IsPrerelease = false };
        }
        else if (!string.IsNullOrWhiteSpace(PackageVersion))
        {
            Version = new InstalledPackageVersion
            {
                InstalledBranch = InstalledBranch, InstalledCommitSha = PackageVersion, IsPrerelease = false
            };
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    ///     Get the launch args host option value.
    /// </summary>
    public string? GetLaunchArgsHost()
    {
        LaunchOption? hostOption = LaunchArgs?.FirstOrDefault(x => x.Name.ToLowerInvariant() == "--host");
        if (hostOption?.OptionValue != null)
        {
            return hostOption.OptionValue as string;
        }

        return hostOption?.DefaultValue as string;
    }

    /// <summary>
    ///     Get the launch args port option value.
    /// </summary>
    public string? GetLaunchArgsPort()
    {
        LaunchOption? portOption = LaunchArgs?.FirstOrDefault(x => x.Name.ToLowerInvariant() == "--port");
        if (portOption?.OptionValue != null)
        {
            return portOption.OptionValue as string;
        }

        return portOption?.DefaultValue as string;
    }

    /// <summary>
    ///     Get the path as a relative sub-path of the relative path.
    ///     If not a sub-path, return null.
    /// </summary>
    public static string? GetSubPath(string relativeTo, string path)
    {
        var relativePath = System.IO.Path.GetRelativePath(relativeTo, path);
        // GetRelativePath returns the path if it's not relative
        if (relativePath == path)
            return null;

        // Further check if the path is a sub-path of the library
        var isSubPath =
            relativePath != "."
            && relativePath != ".."
            && !relativePath.StartsWith(".." + System.IO.Path.DirectorySeparatorChar)
            && !System.IO.Path.IsPathRooted(relativePath);

        return isSubPath ? relativePath : null;
    }

    /// <summary>
    ///     Migrates the old Path to the new LibraryPath.
    ///     If libraryDirectory is null, GlobalConfig.LibraryDir is used.
    /// </summary>
    /// <returns>True if the path was migrated, false otherwise.</returns>
    public bool TryPureMigratePath(string? libraryDirectory = null)
    {
#pragma warning disable CS0618
        var oldPath = Path;
#pragma warning restore CS0618
        if (oldPath == null)
            return false;

        // Check if the path is a sub-path of the library
        var library = libraryDirectory ?? GlobalConfig.LibraryDir;
        var relativePath = GetSubPath(library, oldPath);

        // If so we migrate without any IO operations
        if (relativePath != null)
        {
            LibraryPath = relativePath;
#pragma warning disable CS0618
            Path = null;
#pragma warning restore CS0618
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Check if the old Path can be migrated to the new LibraryPath.
    /// </summary>
    /// <param name="libraryDirectory"></param>
    /// <returns></returns>
    public bool CanPureMigratePath(string? libraryDirectory = null)
    {
#pragma warning disable CS0618
        var oldPath = Path;
#pragma warning restore CS0618
        if (oldPath == null)
            return false;

        // Check if the path is a sub-path of the library
        var library = libraryDirectory ?? GlobalConfig.LibraryDir;
        var relativePath = GetSubPath(library, oldPath);
        return relativePath != null;
    }

    /// <summary>
    ///     Migrate the old Path to the new LibraryPath.
    ///     If libraryDirectory is null, GlobalConfig.LibraryDir is used.
    ///     Will move the package directory to Library/Packages if not relative.
    /// </summary>
    public async Task MigratePath(string? libraryDirectory = null)
    {
#pragma warning disable CS0618
        var oldPath = Path;
#pragma warning restore CS0618
        if (oldPath == null)
            return;

        var libDir = libraryDirectory ?? GlobalConfig.LibraryDir;
        // if old package Path is same as new library, return
        if (oldPath.Replace(DisplayName, "") == libDir)
        {
            // Update the paths
#pragma warning disable CS0618
            Path = null;
#pragma warning restore CS0618
            LibraryPath = System.IO.Path.Combine("Packages", DisplayName);
            return;
        }

        // Try using pure migration first
        if (TryPureMigratePath(libraryDirectory))
            return;

        // If not, we need to move the package directory
        var packageFolderName = new DirectoryInfo(oldPath).Name;

        // Get the new Library/Packages path
        var library = libraryDirectory ?? GlobalConfig.LibraryDir;
        var newPackagesDir = System.IO.Path.Combine(library, "Packages");

        // Get the new target path
        var newPackagePath = System.IO.Path.Combine(newPackagesDir, packageFolderName);
        // Ensure it is not already there, if so, add a suffix until it's not
        var suffix = 2;
        while (Directory.Exists(newPackagePath))
        {
            newPackagePath = System.IO.Path.Combine(newPackagesDir, $"{packageFolderName}-{suffix}");
            suffix++;
        }

        // Move the package directory
        await Task.Run(() => Utilities.CopyDirectory(oldPath, newPackagePath, true));

        // Update the paths
#pragma warning disable CS0618
        Path = null;
#pragma warning restore CS0618
        LibraryPath = System.IO.Path.Combine("Packages", packageFolderName);
    }

    protected bool Equals(InstalledPackage other) => Id.Equals(other.Id);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj.GetType() == GetType() && Equals((InstalledPackage)obj);
    }

    public override int GetHashCode() => Id.GetHashCode();
}