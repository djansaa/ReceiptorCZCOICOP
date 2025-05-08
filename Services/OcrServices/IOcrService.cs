using OpenCvSharp;
using ReceiptorCZCOICOP.Models;

namespace ReceiptorCZCOICOP.Services.OcrServices
{
    /// <summary>
    /// Interface for the OCR service.
    /// </summary>
    internal interface IOcrService
    {
        Task<OcrServiceOutput> OcrAsync(Mat input);
    }
}
