using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace ReceiptorCZCOICOP.Helpers
{
    /// <summary>
    /// Converter for COICOP code to name.
    /// </summary>
    public class CoicopNameConverter : IValueConverter
    {
        // dictionary coicop -> name
        private static readonly Dictionary<string, string> _dict;

        static CoicopNameConverter()
        {
            _dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // read coicop_name.csv from Assets folder
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "coicop_name.csv");

            if (!File.Exists(csvPath)) return;

            // read all lines from the file and parse them into the dict
            foreach (var line in File.ReadAllLines(csvPath))
            {
                // skip header line
                if (line.StartsWith("coicop;")) continue;
                var parts = line.Split(';', 2);

                // if there are two parts add them to the dict
                if (parts.Length == 2 && !_dict.ContainsKey(parts[0].Trim())) _dict[parts[0].Trim()] = parts[1].Trim();
            }
        }

        /// <summary>
        /// Convert COICOP code to name.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>coicop name</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string code && _dict.TryGetValue(code, out var name))
                return name;
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => string.Empty;
    }
}
