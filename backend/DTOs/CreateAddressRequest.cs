namespace WaiMai.Backend.DTOs
{
    public class CreateAddressRequest
    {
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}
