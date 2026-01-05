using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 支付控制器
    /// 处理订单支付完成的请求
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly OrderService _orderService;

        public PaymentController(PaymentService paymentService, OrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        /// <summary>
        /// 完成订单支付
        /// POST /api/payments/{orderId}/pay
        /// </summary>
        [HttpPost("{orderId:int}/pay")]
        public async Task<IActionResult> Pay(int orderId, [FromBody] PaymentRequest request)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new { message = "订单不存在" });
            }

            // 仅允许订单所有者支付
            int userId = GetUserId();
            if (order.UserId != userId)
            {
                return Forbid();
            }

            if (order.PayStatus == Models.Order.PayCompleted)
            {
                return BadRequest(new { message = "订单已支付" });
            }

            await _paymentService.CompletePaymentAsync(orderId, request.PaymentMethod ?? "在线支付");
            return Ok(new { message = "支付成功" });
        }

        private int GetUserId()
        {
            string? sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return string.IsNullOrWhiteSpace(sub) ? 0 : Convert.ToInt32(sub);
        }
    }

    public class PaymentRequest
    {
        public string? PaymentMethod { get; set; }
    }
}
