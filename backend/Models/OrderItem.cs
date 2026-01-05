namespace WaiMai.Backend.Models
{
    /// <summary>
    /// 订单明细实体类
    /// 用于存储订单中每个菜品的详细信息
    /// 数据库表名：OrderDetail
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// 订单明细ID（主键，自增）
        /// </summary>
        public int OrderDetailId { get; set; }

        /// <summary>
        /// 所属订单ID（外键关联Order表）
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// 商品ID（外键关联Food表）
        /// 对应数据库字段：FoodID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 购买数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 下单时的商品单价（记录历史价格，避免后续价格变动影响）
        /// </summary>
        public decimal Price { get; set; }
    }
}
