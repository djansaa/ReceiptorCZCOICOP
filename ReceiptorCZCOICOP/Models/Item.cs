namespace ReceiptorCZCOICOP.Models
{
    /// <summary>
    /// Represents an item in a receipt.
    /// </summary>
    internal class Item
    {
        public string? Name { get; set; }
        public string? ManualName { get; set; }
        public float Value { get; set; }
        public string? Coicop { get; set; }
        public decimal? Confidence { get; set; }
    }
}
