using ReceiptorCZCOICOP.Models;
using System.Globalization;
using System.IO;
using System.Text;

namespace ReceiptorCZCOICOP.Services.DataExportServices
{
    /// <summary>
    /// Service for exporting data to CSV format.
    /// </summary>
    internal class CsvDataExportService : IDataExportService
    {
        /// <summary>
        /// Exports the provided receipts to a CSV file.
        /// </summary>
        /// <param name="receipts">list of receipts to be exported</param>
        /// <param name="filePath">export filepath</param>
        /// <param name="fileName">export filename</param>
        public async Task ExportDataAsync(List<Receipt> receipts, string filePath, string fileName)
        {
            // combine file path and file name
            var fullPath = Path.Combine(filePath, $"{fileName}.csv");

            // check if the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            // writer
            await using var writer = new StreamWriter(fullPath, false, Encoding.UTF8);

            // header
            await writer.WriteLineAsync("id,company,date,currency,product,price,coicop");

            // for each item in the receipt create a line in the csv
            int idx = 1;
            foreach (var r in receipts)
            {
                var date = r.Date?.ToString("dd-MM-yyyy") ?? "";
                var company = EscapeForCsv(r.Company);
                var currency = r.Currency ?? "";

                foreach (var item in r.Items)
                {
                    var product = EscapeForCsv(item.Name);
                    var price = item.Value.ToString("F2", CultureInfo.InvariantCulture);
                    var coicop = item.Coicop ?? "";

                    await writer.WriteLineAsync($"{idx},{company},{date},{currency},{product},{price},{coicop}");
                }

                idx++;
            }
        }

        /// <summary>
        /// Escapes a string for CSV format.
        /// </summary>
        /// <param name="s">input string</param>
        /// <returns>escaped string</returns>
        private static string EscapeForCsv(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.Contains(',') || s.Contains('"') || s.Contains('\n'))
            {
                var escaped = s.Replace("\"", "\"\"");
                return $"\"{escaped}\"";
            }
            return s;
        }
    }
}
