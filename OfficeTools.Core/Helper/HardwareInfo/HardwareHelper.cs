﻿using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Hardware.Info;
using Microsoft.Win32;
using NLog;
using OfficeTools.Core.Extensions;

namespace OfficeTools.Core.Helper.HardwareInfo;

public static partial class HardwareHelper
{
    readonly private static Logger Logger = LogManager.GetCurrentClassLogger();

    private static IReadOnlyList<GpuInfo>? cachedGpuInfos;

    readonly private static Lazy<IHardwareInfo> HardwareInfoLazy =
        new(() => new Hardware.Info.HardwareInfo());

    readonly private static Lazy<bool> IsMemoryInfoAvailableLazy = new(() => TryGetMemoryInfo(out _));

    public static IHardwareInfo HardwareInfo => HardwareInfoLazy.Value;
    public static bool IsMemoryInfoAvailable => IsMemoryInfoAvailableLazy.Value;
    public static bool IsLiveMemoryUsageInfoAvailable => Compat.IsWindows && IsMemoryInfoAvailable;

    private static string RunBashCommand(string command)
    {
        var processInfo = new ProcessStartInfo("bash", "-c \"" + command + "\"")
        {
            UseShellExecute = false, RedirectStandardOutput = true
        };

        Process? process = Process.Start(processInfo);

        process.WaitForExit();

        var output = process.StandardOutput.ReadToEnd();

        return output;
    }

    [SupportedOSPlatform("windows")]
    private static IEnumerable<GpuInfo> IterGpuInfoWindows()
    {
        const string gpuRegistryKeyPath =
            @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";

        using RegistryKey? baseKey = Registry.LocalMachine.OpenSubKey(gpuRegistryKeyPath);

        if (baseKey == null)
            yield break;

        var gpuIndex = 0;

        foreach (var subKeyName in baseKey.GetSubKeyNames().Where(k => k.StartsWith("0")))
        {
            using RegistryKey? subKey = baseKey.OpenSubKey(subKeyName);
            if (subKey != null)
            {
                yield return new GpuInfo
                {
                    Index = gpuIndex++,
                    Name = subKey.GetValue("DriverDesc")?.ToString(),
                    MemoryBytes = Convert.ToUInt64(subKey.GetValue("HardwareInformation.qwMemorySize"))
                };
            }
        }
    }

    [SupportedOSPlatform("linux")]
    private static IEnumerable<GpuInfo> IterGpuInfoLinux()
    {
        var output = RunBashCommand("lspci | grep VGA");
        var gpuLines = output.Split("\n");

        var gpuIndex = 0;

        foreach (var line in gpuLines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var gpuId = line.Split(' ')[0]; // The GPU ID is the first part of the line
            var gpuOutput = RunBashCommand($"lspci -v -s {gpuId}");

            ulong memoryBytes = 0;
            string? name = null;

            // Parse output with regex
            Match match = Regex.Match(gpuOutput, @"VGA compatible controller: ([^\n]*)");
            if (match.Success)
            {
                name = match.Groups[1].Value.Trim();
            }

            match = Regex.Match(gpuOutput, @"prefetchable\) \[size=(\\d+)M\]");
            if (match.Success)
            {
                memoryBytes = ulong.Parse(match.Groups[1].Value) * 1024 * 1024;
            }

            yield return new GpuInfo { Index = gpuIndex++, Name = name, MemoryBytes = memoryBytes };
        }
    }

    [SupportedOSPlatform("macos")]
    private static IEnumerable<GpuInfo> IterGpuInfoMacos()
    {
        HardwareInfo.RefreshVideoControllerList();

        foreach ((var i, VideoController videoController) in HardwareInfo.VideoControllerList.Enumerate())
        {
            var gpuMemoryBytes = 0ul;

            // For arm macs, use the shared system memory
            if (Compat.IsArm)
            {
                gpuMemoryBytes = GetMemoryInfoImplGeneric().TotalPhysicalBytes;
            }

            yield return new GpuInfo { Index = i, Name = videoController.Name, MemoryBytes = gpuMemoryBytes };
        }
    }

    /// <summary>
    ///     Yields GpuInfo for each GPU in the system.
    /// </summary>
    public static IEnumerable<GpuInfo> IterGpuInfo()
    {
        if (Compat.IsWindows)
        {
            return IterGpuInfoWindows();
        }

        if (Compat.IsLinux)
        {
            // Since this requires shell commands, fetch cached value if available.
            if (cachedGpuInfos is not null)
            {
                return cachedGpuInfos;
            }

            // No cache, fetch and cache.
            cachedGpuInfos = IterGpuInfoLinux().ToList();
            return cachedGpuInfos;
        }

        if (Compat.IsMacOS)
        {
            if (cachedGpuInfos is not null)
            {
                return cachedGpuInfos;
            }

            // No cache, fetch and cache.
            cachedGpuInfos = IterGpuInfoMacos().ToList();
            return cachedGpuInfos;
        }

        return Enumerable.Empty<GpuInfo>();
    }

    /// <summary>
    ///     Return true if the system has at least one Nvidia GPU.
    /// </summary>
    public static bool HasNvidiaGpu()
    {
        return IterGpuInfo().Any(gpu => gpu.IsNvidia);
    }

    /// <summary>
    ///     Return true if the system has at least one AMD GPU.
    /// </summary>
    public static bool HasAmdGpu()
    {
        return IterGpuInfo().Any(gpu => gpu.IsAmd);
    }

    // Set ROCm for default if AMD and Linux
    public static bool PreferRocm() => !HasNvidiaGpu() && HasAmdGpu() && Compat.IsLinux;

    // Set DirectML for default if AMD and Windows
    public static bool PreferDirectML() => !HasNvidiaGpu() && HasAmdGpu() && Compat.IsWindows;

    public static bool TryGetMemoryInfo(out MemoryInfo memoryInfo)
    {
        try
        {
            memoryInfo = GetMemoryInfo();
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Failed to get memory info");

            memoryInfo = default;
            return false;
        }
    }

    /// <summary>
    ///     Gets the total and available physical memory in bytes.
    /// </summary>
    public static MemoryInfo GetMemoryInfo() =>
        Compat.IsWindows ? GetMemoryInfoImplWindows() : GetMemoryInfoImplGeneric();

    [SupportedOSPlatform("windows")]
    private static MemoryInfo GetMemoryInfoImplWindows()
    {
        var memoryStatus = new Win32MemoryStatusEx();

        if (!GlobalMemoryStatusEx(ref memoryStatus))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        if (!GetPhysicallyInstalledSystemMemory(out var installedMemoryKb))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return new MemoryInfo
        {
            TotalInstalledBytes = (ulong)installedMemoryKb * 1024,
            TotalPhysicalBytes = memoryStatus.UllTotalPhys,
            AvailablePhysicalBytes = memoryStatus.UllAvailPhys
        };
    }

    private static MemoryInfo GetMemoryInfoImplGeneric()
    {
        HardwareInfo.RefreshMemoryStatus();

        // On macos only TotalPhysical is reported
        if (Compat.IsMacOS)
        {
            return new MemoryInfo
            {
                TotalPhysicalBytes = HardwareInfo.MemoryStatus.TotalPhysical,
                TotalInstalledBytes = HardwareInfo.MemoryStatus.TotalPhysical
            };
        }

        return new MemoryInfo
        {
            TotalPhysicalBytes = HardwareInfo.MemoryStatus.TotalPhysical,
            TotalInstalledBytes = HardwareInfo.MemoryStatus.TotalPhysical,
            AvailablePhysicalBytes = HardwareInfo.MemoryStatus.AvailablePhysical
        };
    }

    /// <summary>
    ///     Gets cpu info
    /// </summary>
    public static Task<CpuInfo> GetCpuInfoAsync() =>
        Compat.IsWindows ? Task.FromResult(GetCpuInfoImplWindows()) : GetCpuInfoImplGenericAsync();

    [SupportedOSPlatform("windows")]
    private static CpuInfo GetCpuInfoImplWindows()
    {
        var info = new CpuInfo();

        using RegistryKey? processorKey = Registry.LocalMachine.OpenSubKey(
            @"Hardware\Description\System\CentralProcessor\0",
            RegistryKeyPermissionCheck.ReadSubTree
        );

        if (processorKey?.GetValue("ProcessorNameString") is string processorName)
        {
            info = info with { ProcessorCaption = processorName.Trim() };
        }

        return info;
    }

    private static Task<CpuInfo> GetCpuInfoImplGenericAsync()
    {
        return Task.Run(() =>
            {
                HardwareInfo.RefreshCPUList();

                if (HardwareInfo.CpuList.FirstOrDefault() is not { } cpu)
                {
                    return default;
                }

                var processorCaption = cpu.Caption.Trim();

                // Try name if caption is empty (like on macos)
                if (string.IsNullOrWhiteSpace(processorCaption))
                {
                    processorCaption = cpu.Name.Trim();
                }

                return new CpuInfo { ProcessorCaption = processorCaption };
            }
        );
    }

    [SupportedOSPlatform("windows")]
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);

    [SupportedOSPlatform("windows")]
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GlobalMemoryStatusEx(ref Win32MemoryStatusEx lpBuffer);
}