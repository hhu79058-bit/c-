namespace WaiMai.Backend.Models
{
    /// <summary>
    /// 商品（菜品）实体类
    /// 用于存储商家提供的菜品信息
    /// 数据库表名：Food
    /// </summary>
    public class Product
    {
        /// <summary>
        /// 商品ID（主键，自增）
        /// 对应数据库字段：FoodID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 所属商家ID（外键关联Merchant表）
        /// </summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// 商品分类ID（外键关联Category表，可为空）
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 商品名称（菜品名称）
        /// 对应数据库字段：FoodName
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 商品价格（单位：元）
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 商品描述（菜品介绍，可为空）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否上架可售（true-上架，false-下架）
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// 商品图片URL
        /// </summary>
        public string? ImageUrl { get; set; }
    }
}
