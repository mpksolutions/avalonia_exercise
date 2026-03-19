using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaExercise.Models;
using SystemColor = System.Drawing.Color;
using AvaloniaColor = Avalonia.Media.Color;

namespace AvaloniaExercise;

public static class ValueConverters
{
    public static FuncValueConverter<SystemColor, SolidColorBrush> ColorToBrush { get; } =
        new(c => new SolidColorBrush(new AvaloniaColor(c.A, c.R, c.G, c.B)));

    public static IValueConverter StatusActiveOpacity { get; } = new StatusActiveOpacityConverter();
}

file sealed class StatusActiveOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TrafficLightStatus status &&
            parameter is string paramStr &&
            Enum.TryParse<TrafficLightStatus>(paramStr, out var target))
            return status == target ? 1.0 : 0.08;
        return 0.08;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
