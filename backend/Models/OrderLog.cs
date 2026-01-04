namespace WaiMai.Backend.Models
{
    public class OrderLog
    {
        public int OrderLogId { get; set; }
        public int OrderId { get; set; }
        public int FromStatus { get; set; }
        public int ToStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Remark { get; set; }
    }
}
