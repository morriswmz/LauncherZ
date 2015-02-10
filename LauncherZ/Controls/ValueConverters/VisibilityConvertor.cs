using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LauncherZ.Controls.ValueConverters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof (Visibility))
                return value;

            if (value is bool)
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is string)
            {
                var str = value as string;
                Visibility v;
                if (Enum.TryParse(str, true, out v))
                {
                    return v;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility))
                return value;

            if (targetType == typeof(bool))
            {
                return (Visibility)value == Visibility.Visible;
            }
            if (targetType == typeof(string))
            {
                return value.ToString();
            }
            return value;
        }
    }
}
