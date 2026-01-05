namespace WaiMai.Backend.Models
{
    /// <summary>
    /// 订单实体类
    /// 用于存储外卖订单的核心信息
    /// </summary>
    public class Order
    {
        /// <summary>
        /// 订单ID（主键，自增）
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// 订单编号（唯一标识符，格式：yyyyMMddHHmmss + 5位随机数）
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// 下单用户ID（外键关联User表）
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 商家ID（外键关联Merchant表）
        /// </summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime OrderTime { get; set; }

        /// <summary>
        /// 订单总金额（单位：元）
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// 订单状态（0-待接单，1-已接单，2-配送中，3-已完成，4-已取消）
        /// </summary>
        public int OrderStatus { get; set; }

        /// <summary>
        /// 支付状态（0-待支付，1-已支付，2-支付失败）
        /// </summary>
        public int PayStatus { get; set; }

        /// <summary>
        /// 配送地址
        /// </summary>
        public string DeliveryAddress { get; set; } = string.Empty;

        // 订单状态常量
        /// <summary>
        /// 订单状态：待接单（刚创建的订单，等待商家确认）
        /// </summary>
        public const int StatusPending = 0;

        /// <summary>
        /// 订单状态：已接单（商家已确认接单，准备制作）
        /// </summary>
        public const int StatusAccepted = 1;

        /// <summary>
        /// 订单状态：配送中（商品已出餐，正在配送）
        /// </summary>
        public const int StatusDelivering = 2;

        /// <summary>
        /// 订单状态：已完成（订单已送达，交易完成）
        /// </summary>
        public const int StatusCompleted = 3;

        /// <summary>
        /// 订单状态：已取消（订单被取消）
        /// </summary>
        public const int StatusCancelled = 4;

        // 支付状态常量
        /// <summary>
        /// 支付状态：待支付（订单已创建，等待用户付款）
        /// </summary>
        public const int PayPending = 0;

        /// <summary>
        /// 支付状态：已支付（用户已成功付款）
        /// </summary>
        public const int PayCompleted = 1;

        /// <summary>
        /// 支付状态：支付失败（支付过程中出现错误）
        /// </summary>
        public const int PayFailed = 2;
    }
}
