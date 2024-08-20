﻿using System.Text.Json.Serialization;

namespace OfficeTools.Core.Python;

public readonly record struct PipPackageInfo(
    string Name,
    string Version,
    string? EditableProjectLocation = null
);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(PipPackageInfo))]
[JsonSerializable(typeof(List<PipPackageInfo>))]
internal partial class PipPackageInfoSerializerContext : JsonSerializerContext;