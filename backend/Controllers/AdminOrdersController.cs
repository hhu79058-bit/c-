using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/orders")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public AdminOrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
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

        [HttpGet("{orderId:int}/logs")]
        public async Task<IActionResult> GetLogs(int orderId, [FromServices] OrderLogService orderLogService)
        {
            var logs = await orderLogService.GetByOrderIdAsync(orderId);
            return Ok(logs);
        }
    }
}
