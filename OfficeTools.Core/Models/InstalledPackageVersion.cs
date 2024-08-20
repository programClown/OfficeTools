﻿using System.Text.Json.Serialization;

namespace OfficeTools.Core.Models;

public class InstalledPackageVersion
{
    public string? InstalledReleaseVersion { get; set; }
    public string? InstalledBranch { get; set; }
    public string? InstalledCommitSha { get; set; }
    public bool IsPrerelease { get; set; }

    [JsonIgnore]
    public bool IsReleaseMode => string.IsNullOrWhiteSpace(InstalledBranch);

    [JsonIgnore]
    public string DisplayVersion => (
        IsReleaseMode
            ? InstalledReleaseVersion
            : string.IsNullOrWhiteSpace(InstalledCommitSha)
                ? InstalledBranch
                : $"{InstalledBranch}@{InstalledCommitSha[..7]}"
    ) ?? string.Empty;
}