using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Neoteric.Desktop.Converters;

public class StringConverter : IValueConverter
{
    public static StringConverter Instance = new StringConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string strValue && parameter is string paramValue)
        {
            return paramValue.StartsWith("!")
                ? strValue != paramValue.Substring(1)
                : strValue == paramValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
