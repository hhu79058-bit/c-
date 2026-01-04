namespace WaiMai.Backend.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int UserType { get; set; }
    }
}
