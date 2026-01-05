using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 管理员订单管理控制器
    /// 仅限管理员访问，用于管理所有订单
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Admin")]  // 仅管理员可访问
    [Route("api/admin/orders")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        /// <summary>
        /// 构造函数：注入订单服务
        /// </summary>
        /// <param name="orderService">订单服务</param>
        public AdminOrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// 获取所有订单
        /// GET /api/admin/orders
        /// </summary>
        /// <returns>所有订单列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// 更新订单状态
        /// PUT /api/admin/orders/{orderId}/status
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="request">新的订单状态</param>
        /// <returns>成功返回200，订单不存在返回404</returns>
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

        /// <summary>
        /// 获取订单状态变更日志
        /// GET /api/admin/orders/{orderId}/logs
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="orderLogService">订单日志服务（方法级注入）</param>
        /// <returns>订单日志列表</returns>
        [HttpGet("{orderId:int}/logs")]
        public async Task<IActionResult> GetLogs(int orderId, [FromServices] OrderLogService orderLogService)
        {
            var logs = await orderLogService.GetByOrderIdAsync(orderId);
            return Ok(logs);
        }
    }
}
