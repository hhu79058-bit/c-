namespace WaiMai.Backend.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public int MerchantId { get; set; }
        public int? CategoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }
}
