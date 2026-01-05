using System.Collections.Generic;

namespace WaiMai.Backend.DTOs
{
    /// <summary>
    /// 创建订单请求DTO
    /// 用于接收客户端提交的订单信息
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// 商家ID（指定从哪个商家下单）
        /// </summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// 配送地址
        /// </summary>
        public string DeliveryAddress { get; set; } = string.Empty;

        /// <summary>
        /// 订单项列表（包含商品ID和数量）
        /// </summary>
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    }
}
