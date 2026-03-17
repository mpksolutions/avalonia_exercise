using Avalonia.Data.Converters;
using Avalonia.Media;
using SystemColor = System.Drawing.Color;
using AvaloniaColor = Avalonia.Media.Color;

namespace AvaloniaExercise;

public static class ValueConverters
{
    public static FuncValueConverter<SystemColor, SolidColorBrush> ColorToBrush { get; } =
        new(c => new SolidColorBrush(new AvaloniaColor(c.A, c.R, c.G, c.B)));
}