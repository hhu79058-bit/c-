namespace WaiMai.Backend.DTOs
{
    /// <summary>
    /// 创建订单时的订单项DTO
    /// 用于表示订单中的单个商品及其数量
    /// </summary>
    public class CreateOrderItemDto
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 购买数量
        /// </summary>
        public int Quantity { get; set; }
    }
}
