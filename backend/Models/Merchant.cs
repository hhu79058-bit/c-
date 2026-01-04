namespace WaiMai.Backend.Models
{
    public class Merchant
    {
        public int MerchantId { get; set; }
        public int UserId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string ShopAddress { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string? ShopIntro { get; set; }
    }
}
