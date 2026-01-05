using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 身份认证控制器
    /// 处理用户登录和注册相关的HTTP请求
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        /// <summary>
        /// 构造函数：注入认证服务
        /// </summary>
        /// <param name="authService">认证服务</param>
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 用户登录接口
        /// POST /api/auth/login
        /// </summary>
        /// <param name="request">登录请求（用户名和密码）</param>
        /// <returns>成功返回200和JWT令牌，失败返回401</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            AuthResponse? response = await _authService.LoginAsync(request);
            if (response == null)
            {
                return Unauthorized(new { message = "用户名或密码错误" });
            }
            return Ok(response);
        }

        /// <summary>
        /// 用户注册接口
        /// POST /api/auth/register
        /// </summary>
        /// <param name="request">注册请求（用户名、密码、手机号等）</param>
        /// <returns>成功返回200和JWT令牌，失败返回400和错误信息</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Response);
        }
    }
}
