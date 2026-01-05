namespace WaiMai.Backend.Models
{
    /// <summary>
    /// 支付记录实体类
    /// 用于存储订单的支付信息
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// 支付记录ID（主键，自增）
        /// </summary>
        public int PaymentId { get; set; }

        /// <summary>
        /// 关联的订单ID（外键关联Order表）
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// 支付金额（单位：元）
        /// </summary>
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// 支付状态（0-待支付，1-已完成，2-支付失败）
        /// </summary>
        public int PaymentStatus { get; set; }

        /// <summary>
        /// 支付时间（可为空，成功支付后记录）
        /// </summary>
        public DateTime? PaymentTime { get; set; }

        /// <summary>
        /// 支付方式（如：微信、支付宝、现金等，可为空）
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// 支付状态常量：待支付
        /// </summary>
        public const int StatusPending = 0;

        /// <summary>
        /// 支付状态常量：已完成
        /// </summary>
        public const int StatusCompleted = 1;

        /// <summary>
        /// 支付状态常量：支付失败
        /// </summary>
        public const int StatusFailed = 2;
    }
}
