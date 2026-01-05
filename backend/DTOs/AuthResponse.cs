namespace WaiMai.Backend.DTOs
{
    /// <summary>
    /// 身份认证响应DTO
    /// 用于返回登录或注册成功后的用户信息和JWT令牌
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// JWT访问令牌（用于后续API请求的身份验证）
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 用户类型（0-顾客，1-商家，2-管理员）
        /// </summary>
        public int UserType { get; set; }
    }
}
