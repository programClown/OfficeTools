using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace OfficeTools;

public static class AlibabaFontSettings
{
    public readonly static string DefaultFontFamily = "fonts:AlibabaPuHuiTi#Alibaba PuHuiTi 3.0";
    public static Uri Key { get; } = new("fonts:AlibabaPuHuiTi", UriKind.Absolute);
    public static Uri Source { get; } = new("avares://OfficeTools/Assets/Fonts/AliBaba", UriKind.Absolute);
}

//HarmonyOS

public static class HarmonyOsFontSettings
{
    public readonly static string DefaultFontFamily = "fonts:HarmonyOS_Sans_SC#HarmonyOS Sans SC";
    public static Uri Key { get; } = new("fonts:HarmonyOS_Sans_SC", UriKind.Absolute);
    public static Uri Source { get; } = new("avares://OfficeTools/Assets/Fonts/HarmonyOS Sans", UriKind.Absolute);
}

public static class AppBuilderExtensions
{
    public static AppBuilder UseAliBabaFontFamily([DisallowNull] this AppBuilder builder)
    {
        builder.With(
            new FontManagerOptions
            {
                DefaultFamilyName = AlibabaFontSettings.DefaultFontFamily,
                FontFallbacks = new[]
                {
                    new FontFallback
                    {
                        FontFamily = new FontFamily(AlibabaFontSettings.DefaultFontFamily)
                    }
                }
            }
        );

        return builder.ConfigureFonts(
            manager =>
                manager.AddFontCollection(
                    new EmbeddedFontCollection(AlibabaFontSettings.Key, AlibabaFontSettings.Source)
                )
        );
    }

    public static AppBuilder UseHarmonyOsFontFamily([DisallowNull] this AppBuilder builder)
    {
        builder.With(
            new FontManagerOptions
            {
                DefaultFamilyName = HarmonyOsFontSettings.DefaultFontFamily,
                FontFallbacks = new[]
                {
                    new FontFallback
                    {
                        FontFamily = new FontFamily(HarmonyOsFontSettings.DefaultFontFamily)
                    }
                }
            }
        );

        return builder.ConfigureFonts(
            manager =>
                manager.AddFontCollection(
                    new EmbeddedFontCollection(HarmonyOsFontSettings.Key, HarmonyOsFontSettings.Source)
                )
        );
    }
}