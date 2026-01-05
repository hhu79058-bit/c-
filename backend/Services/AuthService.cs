using WaiMai.Backend.DTOs;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 身份认证服务类
    /// 负责处理用户登录和注册的业务逻辑
    /// </summary>
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly JwtTokenService _tokenService;

        /// <summary>
        /// 构造函数：注入用户服务和JWT令牌服务
        /// </summary>
        /// <param name="userService">用户服务</param>
        /// <param name="tokenService">JWT令牌服务</param>
        public AuthService(UserService userService, JwtTokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request">登录请求（包含用户名和密码）</param>
        /// <returns>登录成功返回认证响应（包含Token），失败返回null</returns>
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            // 验证用户名和密码
            User? user = await _userService.LoginAsync(request.UserName, request.Password);
            if (user == null)
            {
                return null;  // 用户名或密码错误
            }

            // 生成JWT令牌并返回
            return new AuthResponse
            {
                Token = _tokenService.CreateToken(user),
                UserId = user.UserId,
                UserName = user.UserName,
                UserType = user.UserType
            };
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="request">注册请求（包含用户名、密码、手机号等）</param>
        /// <returns>成功标志、提示消息、认证响应（成功时）</returns>
        public async Task<(bool Success, string Message, AuthResponse? Response)> RegisterAsync(RegisterRequest request)
        {
            // 检查用户名是否已存在
            bool nameExists = await _userService.IsUserNameExistsAsync(request.UserName);
            if (nameExists)
            {
                return (false, "用户名已存在", null);
            }

            // 检查手机号是否已存在
            bool phoneExists = await _userService.IsPhoneNumberExistsAsync(request.PhoneNumber);
            if (phoneExists)
            {
                return (false, "手机号已存在", null);
            }

            // 创建用户对象
            User user = new User
            {
                UserName = request.UserName,
                Password = request.Password,  // 注意：应在此处进行密码哈希处理
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                UserType = request.UserType
            };

            // 插入数据库
            int userId = await _userService.CreateUserAsync(user);
            if (userId <= 0)
            {
                return (false, "注册失败", null);
            }

            // 注册成功，生成JWT令牌并返回
            user.UserId = userId;
            return (true, "注册成功", new AuthResponse
            {
                Token = _tokenService.CreateToken(user),
                UserId = user.UserId,
                UserName = user.UserName,
                UserType = user.UserType
            });
        }
    }
}
