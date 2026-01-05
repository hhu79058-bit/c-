using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Models;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 管理员商品管理控制器
    /// 仅限管理员访问，可管理所有商家的商品
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Admin")]  // 仅管理员可访问
    [Route("api/admin/products")]
    public class AdminProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        /// <summary>
        /// 构造函数：注入商品服务
        /// </summary>
        /// <param name="productService">商品服务</param>
        public AdminProductsController(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// 根据商家ID查询商品列表
        /// GET /api/admin/products?merchantId=1
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

        /// <summary>
        /// 为指定商家创建新商品
        /// POST /api/admin/products
        /// </summary>
        /// <param name="request">商品创建请求（包含商家ID）</param>
        /// <returns>成功返回200和商品ID，失败返回400</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdminCreateProductRequest request)
        {
            Product product = new Product
            {
                MerchantId = request.MerchantId,
                ProductName = request.ProductName,
                Price = request.Price,
                Description = request.Description,
                IsAvailable = request.IsAvailable,
                ImageUrl = request.ImageUrl
            };

            int productId = await _productService.CreateAsync(product);
            if (productId <= 0)
            {
                return BadRequest(new { message = "新增失败" });
            }

            return Ok(new { productId });
        }

        /// <summary>
        /// 更新商品信息
        /// PUT /api/admin/products/{productId}
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="request">商品更新请求</param>
        /// <returns>成功返回200，失败返回404</returns>
        [HttpPut("{productId:int}")]
        public async Task<IActionResult> Update(int productId, [FromBody] AdminUpdateProductRequest request)
        {
            Product product = new Product
            {
                ProductId = productId,
                MerchantId = request.MerchantId,
                ProductName = request.ProductName,
                Price = request.Price,
                Description = request.Description,
                IsAvailable = request.IsAvailable,
                ImageUrl = request.ImageUrl
            };

            bool success = await _productService.UpdateAsync(product);
            if (!success)
            {
                return NotFound(new { message = "菜品不存在或更新失败" });
            }

            return Ok(new { message = "更新成功" });
        }

        /// <summary>
        /// 快速切换商品上架/下架状态
        /// PATCH /api/admin/products/{productId}/availability
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="request">上架状态请求（包含商家ID和目标状态）</param>
        /// <returns>成功返回200，失败返回404</returns>
        [HttpPatch("{productId:int}/availability")]
        public async Task<IActionResult> ToggleAvailability(int productId, [FromBody] AdminToggleProductRequest request)
        {
            bool success = await _productService.ToggleAvailabilityAsync(productId, request.MerchantId, request.IsAvailable);
            if (!success)
            {
                return NotFound(new { message = "菜品不存在或更新失败" });
            }

            return Ok(new { message = "状态已更新" });
        }
    }
}
