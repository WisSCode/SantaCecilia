using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace frontend.Converters
{
    public class FirstCharConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var s = value as string;
            if (string.IsNullOrEmpty(s)) return "?";
            return s.Substring(0, 1).ToUpperInvariant();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
