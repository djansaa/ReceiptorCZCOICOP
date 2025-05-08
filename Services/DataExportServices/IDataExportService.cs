using ReceiptorCZCOICOP.Models;

namespace ReceiptorCZCOICOP.Services.DataExportServices
{
    /// <summary>
    /// Interface for the data export service.
    /// </summary>
    internal interface IDataExportService
    {
        Task ExportDataAsync(List<Receipt> receipts, string filePath, string fileName);
    }
}
