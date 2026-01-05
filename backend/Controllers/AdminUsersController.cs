using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.Models;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")] // 仅管理员可访问
    public class AdminUsersController : ControllerBase
    {
        private readonly UserService _userService;

        public AdminUsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            
            // 转换为前端需要的格式，隐藏敏感信息（如密码）
            var result = users.Select(u => new
            {
                u.UserId,
                u.UserName,
                u.PhoneNumber,
                Role = u.UserType switch
                {
                    0 => "用户",
                    1 => "商家",
                    2 => "管理员",
                    _ => "未知"
                },
                // 暂时没有注册时间字段，用模拟或空值代替，或者如果后续加上了再填
                RegisterTime = "2023-01-01" // 这里暂时写死，因为 User 模型里没有 RegisterTime
            });

            return Ok(result);
        }
    }
}
