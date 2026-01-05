using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WaiMai.Backend.Models;
using WaiMai.Backend.Options;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// JWT令牌服务类
    /// 负责生成和管理JWT访问令牌
    /// </summary>
    public class JwtTokenService
    {
        /// <summary>
        /// JWT配置选项
        /// </summary>
        private readonly JwtOptions _options;

        /// <summary>
        /// 构造函数：注入JWT配置选项
        /// </summary>
        /// <param name="options">JWT配置选项（从appsettings.json读取）</param>
        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// 为用户创建JWT令牌
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <returns>JWT令牌字符串</returns>
        public string CreateToken(User user)
        {
            // 定义令牌中包含的声明（Claims）
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),      // 用户ID（标准声明）
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),        // 用户ID（ASP.NET标准）
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),        // 用户名
                new Claim(ClaimTypes.Role, RoleHelper.GetRoleName(user.UserType))    // 角色（用于授权）
            };

            // 创建签名密钥
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 构建JWT令牌
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _options.Issuer,           // 颁发者
                audience: _options.Audience,       // 接收者
                claims: claims,                    // 声明列表
                expires: DateTime.UtcNow.AddHours(12),  // 过期时间（12小时）
                signingCredentials: creds          // 签名凭证
            );

            // 序列化为字符串并返回
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
