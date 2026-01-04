namespace WaiMai.Backend.DTOs
{
    public class AdminCreateProductRequest
    {
        public int MerchantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }
}
