namespace WaiMai.Backend.DTOs
{
    /// <summary>
    /// 管理员更新商品请求DTO
    /// 用于管理员修改商品信息
    /// </summary>
    public class AdminUpdateProductRequest
    {
        /// <summary>
        /// 所属商家ID（用于权限验证）
        /// </summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 商品价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 商品描述（可选）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// 商品图片URL
        /// </summary>
        public string? ImageUrl { get; set; }
    }
}
