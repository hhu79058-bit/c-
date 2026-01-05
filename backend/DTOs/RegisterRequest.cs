namespace WaiMai.Backend.DTOs
{
    /// <summary>
    /// 用户注册请求DTO
    /// 用于接收客户端提交的注册信息
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// 用户名（需唯一）
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 手机号码（需唯一）
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 用户类型（0-顾客，1-商家，2-管理员）
        /// </summary>
        public int UserType { get; set; }
    }
}
