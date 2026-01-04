using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    [ApiController]
    [Authorize(Roles = "Merchant")]
    [Route("api/merchant/orders")]
    public class MerchantOrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly MerchantService _merchantService;

        public MerchantOrdersController(OrderService orderService, MerchantService merchantService)
        {
            _orderService = orderService;
            _merchantService = merchantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            int userId = GetUserId();
            var merchant = await _merchantService.GetByUserIdAsync(userId);
            if (merchant == null)
            {
                return NotFound(new { message = "未找到商家信息" });
            }

            var orders = await _orderService.GetOrdersByMerchantAsync(merchant.MerchantId);
            return Ok(orders);
        }

        [HttpPut("{orderId:int}/status")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            bool success = await _orderService.UpdateOrderStatusAsync(orderId, request.OrderStatus);
            if (!success)
            {
                return NotFound(new { message = "订单不存在" });
            }
            return Ok(new { message = "订单状态已更新" });
        }

        private int GetUserId()
        {
            string? sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return string.IsNullOrWhiteSpace(sub) ? 0 : Convert.ToInt32(sub);
        }
    }
}
