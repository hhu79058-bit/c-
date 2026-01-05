namespace WaiMai.Backend.DTOs
{
    /// <summary>
    /// 更新订单状态请求DTO
    /// 用于商家或管理员修改订单状态
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        /// 新的订单状态（0-待接单，1-已接单，2-配送中，3-已完成，4-已取消）
        /// </summary>
        public int OrderStatus { get; set; }
    }
}
