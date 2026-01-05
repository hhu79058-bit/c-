namespace WaiMai.Backend.Options
{
    /// <summary>
    /// JWT配置选项类
    /// 用于存储JWT令牌生成和验证所需的配置参数
    /// 从appsettings.json的"Jwt"节读取
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// 令牌颁发者（Issuer）
        /// 表示JWT令牌的签发方
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 令牌接收者（Audience）
        /// 表示JWT令牌的预期使用方
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// 密钥（用于签名和验证JWT令牌）
        /// 生产环境必须使用强密钥（至少32字符）并通过环境变量配置
        /// </summary>
        public string Key { get; set; } = string.Empty;
    }
}
