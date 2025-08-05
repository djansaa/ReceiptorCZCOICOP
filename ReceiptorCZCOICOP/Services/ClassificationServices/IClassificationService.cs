using ReceiptorCZCOICOP.Models;

namespace ReceiptorCZCOICOP.Services.ClassificationServices
{
    /// <summary>
    /// Interface for the classification service.
    /// </summary>
    internal interface IClassificationService
    {
        Task<ClassificationServiceOutput> ClassifyAsync(string productName);
    }
}
