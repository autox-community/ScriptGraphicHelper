using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ScriptGraphicHelper.Converters
{
    class ScaleFactorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }
            var scaleFactor = (double)value;
            return scaleFactor switch
            {
                0.3 => 0,
                0.4 => 1,
                0.5 => 2,
                0.6 => 3,
                0.7 => 4,
                0.8 => 5,
                0.9 => 6,
                1.0 => 7,
                1.2 => 8,
                1.4 => 9,
                1.6 => 10,
                1.8 => 11,
                2.0 => 12,
                _ => 7
            };
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }
            var scaleFactor = (int)value;
            return scaleFactor switch
            {
                0 => 0.3,
                1 => 0.4,
                2 => 0.5,
                3 => 0.6,
                4 => 0.7,
                5 => 0.8,
                6 => 0.9,
                7 => 1.0,
                8 => 1.2,
                9 => 1.4,
                10 => 1.6,
                11 => 1.8,
                12 => 2.0,
                _ => 1.0
            };
        }
    }
}
