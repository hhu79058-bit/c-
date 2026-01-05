using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 商家查询控制器
    /// 允许所有用户（包括未登录）查看商家列表
    /// </summary>
    [ApiController]
    [Route("api/merchants")]
    public class MerchantsController : ControllerBase
    {
        private readonly MerchantService _merchantService;

        /// <summary>
        /// 构造函数：注入商家服务
        /// </summary>
        /// <param name="merchantService">商家服务</param>
        public MerchantsController(MerchantService merchantService)
        {
            _merchantService = merchantService;
        }

        /// <summary>
        /// 获取所有商家列表
        /// GET /api/merchants
        /// </summary>
        /// <returns>商家列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var merchants = await _merchantService.GetAllAsync();
            return Ok(merchants);
        }
    }
}
