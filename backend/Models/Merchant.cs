namespace WaiMai.Backend.Models
{
    /// <summary>
    /// 商家实体类
    /// 用于存储商家店铺的详细信息
    /// 一个用户（UserType=1）可以对应一个商家账号
    /// </summary>
    public class Merchant
    {
        /// <summary>
        /// 商家ID（主键，自增）
        /// </summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// 关联的用户ID（外键关联User表）
        /// 用于登录认证和权限管理
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// 店铺地址
        /// </summary>
        public string ShopAddress { get; set; } = string.Empty;

        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactPhone { get; set; } = string.Empty;

        /// <summary>
        /// 店铺介绍（可为空）
        /// </summary>
        public string? ShopIntro { get; set; }

        /// <summary>
        /// 店铺Logo图片URL
        /// </summary>
        public string? LogoUrl { get; set; }
    }
}
