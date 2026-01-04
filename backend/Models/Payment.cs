namespace WaiMai.Backend.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal PaymentAmount { get; set; }
        public int PaymentStatus { get; set; }
        public DateTime? PaymentTime { get; set; }
        public string? PaymentMethod { get; set; }

        public const int StatusPending = 0;
        public const int StatusCompleted = 1;
        public const int StatusFailed = 2;
    }
}
