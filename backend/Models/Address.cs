namespace WaiMai.Backend.Models
{
    /// <summary>
    /// 收货地址实体类
    /// 用于存储用户的收货地址信息
    /// 支持多地址管理和默认地址设置
    /// </summary>
    public class Address
    {
        /// <summary>
        /// 地址ID（主键，自增）
        /// </summary>
        public int AddressId { get; set; }

        /// <summary>
        /// 所属用户ID（外键关联User表）
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string RecipientName { get; set; } = string.Empty;

        /// <summary>
        /// 收货人手机号
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// 完整收货地址
        /// </summary>
        public string FullAddress { get; set; } = string.Empty;

        /// <summary>
        /// 是否为默认地址（每个用户只能有一个默认地址）
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
