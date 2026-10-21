using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RoyalDining.Desktop.Converters;

public class WorkAreaMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // WorkArea adalah area layar minus taskbar.
        // Kita kembalikan Margin 0 agar window pas dengan WorkArea.
        // Namun karena WPF kadang menghitung posisi window meleset saat maximize,
        // kita gunakan trik: set margin kecil (misal 7px) agar tidak menutupi taskbar.
        return new Thickness(7);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}