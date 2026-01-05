using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 商家订单管理控制器
    /// 仅限商家访问，用于管理自己店铺的订单
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Merchant")]  // 仅商家可访问
    [Route("api/merchant/orders")]
    public class MerchantOrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly MerchantService _merchantService;

        /// <summary>
        /// 构造函数：注入订单服务和商家服务
        /// </summary>
        /// <param name="orderService">订单服务</param>
        /// <param name="merchantService">商家服务</param>
        public MerchantOrdersController(OrderService orderService, MerchantService merchantService)
        {
            _orderService = orderService;
            _merchantService = merchantService;
        }

        /// <summary>
        /// 查询当前商家的所有订单
        /// GET /api/merchant/orders
        /// </summary>
        /// <returns>订单列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            int userId = GetUserId();
            // 根据用户ID查找对应的商家信息
            var merchant = await _merchantService.GetByUserIdAsync(userId);
            if (merchant == null)
            {
                return NotFound(new { message = "未找到商家信息" });
            }

            var orders = await _orderService.GetOrdersByMerchantAsync(merchant.MerchantId);
            return Ok(orders);
        }

        /// <summary>
        /// 更新订单状态（仅限本商家的订单）
        /// PUT /api/merchant/orders/{orderId}/status
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="request">新的订单状态</param>
        /// <returns>成功返回200，无权限返回403，订单不存在返回404</returns>
        [HttpPut("{orderId:int}/status")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            int userId = GetUserId();
            var merchant = await _merchantService.GetByUserIdAsync(userId);
            if (merchant == null)
            {
                return NotFound(new { message = "未找到商家信息" });
            }

            // 验证订单是否属于当前商家（防止跨商家修改订单）
            int? orderMerchantId = await _orderService.GetOrderMerchantIdAsync(orderId);
            if (orderMerchantId == null)
            {
                return NotFound(new { message = "订单不存在" });
            }

            if (orderMerchantId != merchant.MerchantId)
            {
                return Forbid();  // 无权操作其他商家的订单
            }

            bool success = await _orderService.UpdateOrderStatusAsync(orderId, request.OrderStatus);
            if (!success)
            {
                return NotFound(new { message = "订单不存在" });
            }
            return Ok(new { message = "订单状态已更新" });
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
