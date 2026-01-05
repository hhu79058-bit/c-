using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 商品（菜品）查询控制器
    /// 允许所有用户（包括未登录）查看商家的商品列表
    /// </summary>
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        /// <summary>
        /// 构造函数：注入商品服务
        /// </summary>
        /// <param name="productService">商品服务</param>
        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// 根据商家ID查询该商家的所有商品
        /// GET /api/products?merchantId=1
        /// </summary>
        /// <param name="merchantId">商家ID（查询参数）</param>
        /// <returns>商品列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetByMerchant([FromQuery] int merchantId)
        {
            if (merchantId <= 0)
            {
                return BadRequest(new { message = "merchantId 不能为空" });
            }

            var products = await _productService.GetByMerchantIdAsync(merchantId);
            return Ok(products);
        }
    }
}
