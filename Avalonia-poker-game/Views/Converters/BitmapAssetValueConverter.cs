using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;


namespace Avalonia_poker_game.Views.Converters;

/// <summary>
/// Converts an "avares://..." URI string to an Avalonia Bitmap.
/// Returns null (Image shows nothing) if the asset file doesn't exist yet.
/// </summary>
public class BitmapAssetValueConverter : IValueConverter
{
    public static readonly BitmapAssetValueConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string rawUri || string.IsNullOrWhiteSpace(rawUri))
            return null;

        try
        {
            var uri    = new Uri(rawUri);
            var stream = AssetLoader.Open(uri);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
