namespace WaiMai.Backend.DTOs
{
    /// <summary>
    /// 用户登录请求DTO
    /// 用于接收客户端提交的登录信息
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
