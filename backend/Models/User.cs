namespace WaiMai.Backend.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int UserType { get; set; }

        public const int TypeCustomer = 0;
        public const int TypeMerchant = 1;
        public const int TypeAdmin = 2;
    }
}
