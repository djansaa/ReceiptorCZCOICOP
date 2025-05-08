using ReceiptorCZCOICOP.Models;

namespace ReceiptorCZCOICOP.Services.DataExtractionServices
{
    /// <summary>
    /// Interface for the data extraction service.
    /// </summary>
    internal interface IDataExtractionService
    {
        Task<Receipt?> ExtractDataAsync(string receiptText);
    }
}
