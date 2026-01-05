namespace WaiMai.Backend.DTOs
{
    /// <summary>
    /// 管理员切换商品上架状态请求DTO
    /// 用于管理员快速上架或下架商品
    /// </summary>
    public class AdminToggleProductRequest
    {
        /// <summary>
        /// 所属商家ID（用于权限验证）
        /// </summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// 目标上架状态（true-上架，false-下架）
        /// </summary>
        public bool IsAvailable { get; set; }
    }
}
