namespace WaiMai.Backend.Models
{
    /// <summary>
    /// 用户实体类
    /// 用于存储系统中所有用户的基本信息，包括顾客、商家和管理员
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户ID（主键，自增）
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户名（登录账号，唯一）
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 密码（注意：当前为明文存储，建议使用哈希加密）
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 手机号码（唯一，用于联系和验证）
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// 地址信息
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 用户类型（0-顾客，1-商家，2-管理员）
        /// </summary>
        public int UserType { get; set; }

        /// <summary>
        /// 用户类型常量：顾客
        /// </summary>
        public const int TypeCustomer = 0;

        /// <summary>
        /// 用户类型常量：商家
        /// </summary>
        public const int TypeMerchant = 1;

        /// <summary>
        /// 用户类型常量：管理员
        /// </summary>
        public const int TypeAdmin = 2;
    }
}
