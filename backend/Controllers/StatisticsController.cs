using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 统计数据控制器
    /// 仅限管理员访问，用于查看系统统计信息
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Admin")]  // 仅管理员可访问
    [Route("api/admin/statistics")]
    public class StatisticsController : ControllerBase
    {
        private readonly StatisticsService _statisticsService;

        /// <summary>
        /// 构造函数：注入统计服务
        /// </summary>
        /// <param name="statisticsService">统计服务</param>
        public StatisticsController(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        /// <summary>
        /// 获取系统统计汇总数据
        /// GET /api/admin/statistics
        /// 返回订单总数、总营业额、今日订单数等
        /// </summary>
        /// <returns>统计数据字典</returns>
        [HttpGet]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _statisticsService.GetSummaryAsync();
            return Ok(summary);
        }
    }
}
