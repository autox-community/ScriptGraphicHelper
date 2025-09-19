using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace ScriptGraphicHelper.Converters
{
    public class Color2HexConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }
            var color = (Color)value;
            return string.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var str = (string)value;
            if (str.IndexOf('#') == -1)
            {
                str = "#" + str;
            }
            return Color.Parse(str.PadRight(7, '0'));
        }
    }
    class Color2BrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }
            var color = (Color)value;
            return Brush.Parse(string.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2")));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }
            var brush = (Brush)value;
            return Color.Parse(brush.ToString());
        }
    }

    public class _Color2HexConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }
            var color = (Color)value;
            return string.Format("#{0}{1}{2}{3}", color.A.ToString("X2"), color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var str = (string)value;
            if (str.IndexOf('#') == -1)
            {
                str = "#" + str;
            }
            return Color.Parse(str.PadRight(9, '0'));
        }
    }
    class _Color2BrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }
            var color = (Color)value;
            return Brush.Parse(string.Format("#{0}{1}{2}{3}", color.A.ToString("X2"), color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2")));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var brush = (Brush)value;
            return Color.Parse(brush.ToString());
        }
    }
}
