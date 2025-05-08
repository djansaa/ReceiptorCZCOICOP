using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ReceiptorCZCOICOP.Helpers
{
    /// <summary>
    /// Converter for confidence level to color.
    /// </summary>
    public class ConfidenceColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush Green = new(Color.FromRgb(144, 238, 144));
        private static readonly SolidColorBrush Yellow = new(Color.FromRgb(255, 255, 128));
        private static readonly SolidColorBrush Red = new(Color.FromRgb(255, 189, 204));
        private static readonly SolidColorBrush Transparent = Brushes.Transparent;

        /// <summary>
        /// Convert confidence level to color.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>color brush</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Transparent;

            double conf;
            try
            {
                conf = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return Transparent;
            }

            // return color based on confidence level
            if (conf >= 0.98) return Yellow;
            if (conf >= 0.95) return Red;
            if (conf == -1) return Green;
            return Transparent;
        }

        public object ConvertBack(object value, Type t, object p, CultureInfo c)
        {
            return Transparent;
        }
    }
}
