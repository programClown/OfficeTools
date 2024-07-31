using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace OfficeTools.Converters;

public class IconNameToPathConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            var imageName = new Bitmap(AssetLoader.Open(new Uri($"avares://OfficeTools/Assets/{s}.png")));
            return imageName;
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        AvaloniaProperty.UnsetValue;
}