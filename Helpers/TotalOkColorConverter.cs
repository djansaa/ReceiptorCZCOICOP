using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ReceiptorCZCOICOP.Helpers
{
    /// <summary>
    /// Converter for TotalOk property to color.
    /// </summary>
    class TotalOkColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush Green = new(Color.FromRgb(144, 238, 144));
        private static readonly SolidColorBrush Red = new(Color.FromRgb(255, 189, 204));

        /// <summary>
        /// Convert TotalOk property to color.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="t"></param>
        /// <param name="p"></param>
        /// <param name="c"></param>
        /// <returns>color brush</returns>
        public object Convert(object value, Type t, object p, CultureInfo c) =>
            value is true ? Green : Red;

        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }
}
