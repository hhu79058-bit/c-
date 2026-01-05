using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 订单控制器（普通用户）
    /// 处理用户查看和创建订单的HTTP请求
    /// 需要JWT身份验证
    /// </summary>
    [ApiController]
    [Authorize]  // 要求所有接口都需要登录
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        /// <summary>
        /// 构造函数：注入订单服务
        /// </summary>
        /// <param name="orderService">订单服务</param>
        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// 查询当前用户的所有订单
        /// GET /api/orders/my
        /// </summary>
        /// <returns>订单列表</returns>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            int userId = GetUserId();
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            return Ok(orders);
        }

        /// <summary>
        /// 创建新订单
        /// POST /api/orders
        /// </summary>
        /// <param name="request">订单创建请求</param>
        /// <returns>成功返回201和订单ID，失败返回400</returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request.Items.Count == 0)
            {
                return BadRequest(new { message = "订单项不能为空" });
            }

            int userId = GetUserId();
            try
            {
                int orderId = await _orderService.CreateOrderAsync(userId, request);
                return CreatedAtAction(nameof(GetMyOrders), new { id = orderId }, new { orderId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 从JWT令牌中提取当前用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        private int GetUserId()
        {
            string? sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return string.IsNullOrWhiteSpace(sub) ? 0 : Convert.ToInt32(sub);
        }
    }
}
