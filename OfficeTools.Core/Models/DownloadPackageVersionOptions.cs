namespace OfficeTools.Core.Models;

public class DownloadPackageVersionOptions
{
    public string? BranchName { get; set; }
    public string? CommitHash { get; set; }
    public string? VersionTag { get; set; }
    public bool IsLatest { get; set; }
    public bool IsPrerelease { get; set; }

    public string ReadableVersionString => GetReadableVersionString();

    public string GetReadableVersionString() =>
        !string.IsNullOrWhiteSpace(VersionTag) ? VersionTag : $"{BranchName}@{CommitHash?[..7]}";
}