using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace ReceiptorCZCOICOP.Helpers
{
    /// <summary>
    /// Converter for inverting boolean values.
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : DependencyProperty.UnsetValue;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : DependencyProperty.UnsetValue;
    }
}
