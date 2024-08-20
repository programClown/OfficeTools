﻿using System.Diagnostics.CodeAnalysis;
using OfficeTools.Core.Models.FileInterfaces;

namespace OfficeTools.Core.Models;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class GlobalConfig
{
    private static DirectoryPath? libraryDir;

    /// <summary>
    ///     Absolute path to the library directory.
    ///     Needs to be set by SettingsManager.TryFindLibrary() before being accessed.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public static DirectoryPath LibraryDir
    {
        get
        {
            if (libraryDir is null)
            {
                throw new NullReferenceException("GlobalConfig.LibraryDir was not set before being accessed.");
            }

            return libraryDir;
        }
        set => libraryDir = value;
    }

    /// <summary>
    ///     Full path to the %APPDATA% directory.
    ///     Usually C:\Users\{username}\AppData\Roaming
    /// </summary>
    public static DirectoryPath AppDataDir { get; } =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    /// <summary>
    ///     Full path to the fixed home directory.
    ///     Currently %APPDATA%\StabilityMatrix
    /// </summary>
    public static DirectoryPath HomeDir { get; set; } = AppDataDir.JoinDir("StabilityMatrix");
}