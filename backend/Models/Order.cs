namespace WaiMai.Backend.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int MerchantId { get; set; }
        public DateTime OrderTime { get; set; }
        public decimal OrderAmount { get; set; }
        public int OrderStatus { get; set; }
        public int PayStatus { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;

        public const int StatusPending = 0;
        public const int StatusAccepted = 1;
        public const int StatusDelivering = 2;
        public const int StatusCompleted = 3;
        public const int StatusCancelled = 4;

        public const int PayPending = 0;
        public const int PayCompleted = 1;
        public const int PayFailed = 2;
    }
}
