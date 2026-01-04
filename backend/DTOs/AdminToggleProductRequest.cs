namespace WaiMai.Backend.DTOs
{
    public class AdminToggleProductRequest
    {
        public int MerchantId { get; set; }
        public bool IsAvailable { get; set; }
    }
}
