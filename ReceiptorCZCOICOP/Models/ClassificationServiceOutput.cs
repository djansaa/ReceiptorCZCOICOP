namespace ReceiptorCZCOICOP.Models
{
    /// <summary>
    /// Represents the output of the classification service.
    /// </summary>
    internal class ClassificationServiceOutput
    {
        public string? Coicop { get; set; } = null;
        public decimal? Confidence { get; set; } = null;
    }
}
