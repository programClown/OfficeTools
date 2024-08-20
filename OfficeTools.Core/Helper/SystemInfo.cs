﻿using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OfficeTools.Core.Helper;

public static class SystemInfo
{
    public const long Gibibyte = 1024 * 1024 * 1024;
    public const long Mebibyte = 1024 * 1024;

    [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
    public static extern bool ShouldUseDarkMode();

    public static long? GetDiskFreeSpaceBytes(string path)
    {
        try
        {
            var drive = new DriveInfo(path);
            return drive.AvailableFreeSpace;
        }
        catch (Exception e)
        {
            // LogManager.GetCurrentClassLogger().Error(e);
            Debug.Write(e);
        }

        return null;
    }
}