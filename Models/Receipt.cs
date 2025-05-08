namespace ReceiptorCZCOICOP.Models
{
    /// <summary>
    /// Represents a receipt with its details.
    /// </summary>
    internal class Receipt
    {
        public string? Company { get; set; } = "";
        public DateTime? Date { get; set; }
        public string? Currency { get; set; } = "";
        public List<Item> Items { get; set; } = new List<Item>();
        public float Total { get; set; } = 0;
    }
}
