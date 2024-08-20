using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace OfficeTools.Core.Models.FileInterfaces;

[PublicAPI]
[Localizable(false)]
public class FileSystemPath : IEquatable<FileSystemPath>, IFormattable
{
    protected FileSystemPath(string path)
    {
        FullPath = path;
    }

    protected FileSystemPath(FileSystemPath path)
        : this(path.FullPath)
    {
    }

    protected FileSystemPath(params string[] paths)
        : this(Path.Combine(paths))
    {
    }

    public string FullPath { get; }

    /// <inheritdoc />
    public bool Equals(FileSystemPath? other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return string.Equals(
            GetNormalizedPath(FullPath),
            GetNormalizedPath(other.FullPath),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
        );
    }

    /// <inheritdoc />
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(format, formatProvider);

    /// <inheritdoc />
    public override string ToString() => FullPath;

    /// <summary>
    ///     Overridable IFormattable.ToString method.
    ///     By default, returns <see cref="FullPath" />.
    /// </summary>
    protected virtual string ToString(string? format, IFormatProvider? formatProvider) => FullPath;

    public static bool operator ==(FileSystemPath? left, FileSystemPath? right) => Equals(left, right);

    public static bool operator !=(FileSystemPath? left, FileSystemPath? right) => !Equals(left, right);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (GetType() != obj.GetType())
            return false;

        return Equals((FileSystemPath)obj);
    }

    /// <summary>
    ///     Normalize a path to a consistent format for comparison.
    /// </summary>
    /// <param name="path">Path to normalize.</param>
    /// <returns>Normalized path.</returns>
    [return: NotNullIfNotNull(nameof(path))]
    private static string? GetNormalizedPath(string? path)
    {
        // Return null or empty paths as-is
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri? uri))
        {
            if (uri.IsAbsoluteUri)
            {
                path = uri.LocalPath;
            }
        }

        // Get full path if possible, ignore errors like invalid chars or too long
        try
        {
            path = Path.GetFullPath(path);
        }
        catch (SystemException)
        {
        }

        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType().GetHashCode(), FullPath.GetHashCode());

    // Implicit conversions to and from string
    public static implicit operator string(FileSystemPath path) => path.FullPath;

    public static implicit operator FileSystemPath(string path) => new(path);
}