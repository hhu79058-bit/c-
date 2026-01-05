using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Models;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 商家商品管理控制器
    /// 仅限商家访问，用于管理自己店铺的商品
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Merchant")]  // 仅商家可访问
    [Route("api/merchant/products")]
    public class MerchantProductsController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly MerchantService _merchantService;

        /// <summary>
        /// 构造函数：注入商品服务和商家服务
        /// </summary>
        /// <param name="productService">商品服务</param>
        /// <param name="merchantService">商家服务</param>
        public MerchantProductsController(ProductService productService, MerchantService merchantService)
        {
            _productService = productService;
            _merchantService = merchantService;
        }

        /// <summary>
        /// 查询当前商家的所有商品
        /// GET /api/merchant/products
        /// </summary>
        /// <returns>商品列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetMyProducts()
        {
            int userId = GetUserId();
            Merchant? merchant = await _merchantService.GetByUserIdAsync(userId);
            if (merchant == null)
            {
                return NotFound(new { message = "未找到商家信息" });
            }

            var products = await _productService.GetByMerchantIdAsync(merchant.MerchantId);
            return Ok(products);
        }

        /// <summary>
        /// 创建新商品
        /// POST /api/merchant/products
        /// </summary>
        /// <param name="request">商品创建请求</param>
        /// <returns>成功返回200和商品ID，失败返回400或404</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdminCreateProductRequest request)
        {
            int userId = GetUserId();
            Merchant? merchant = await _merchantService.GetByUserIdAsync(userId);
            if (merchant == null)
            {
                return NotFound(new { message = "未找到商家信息" });
            }

            Product product = new Product
            {
                MerchantId = merchant.MerchantId,  // 自动使用当前商家的ID
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
        /// PUT /api/merchant/products/{productId}
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="request">商品更新请求</param>
        /// <returns>成功返回200，失败返回404</returns>
        [HttpPut("{productId:int}")]
        public async Task<IActionResult> Update(int productId, [FromBody] AdminUpdateProductRequest request)
        {
            int userId = GetUserId();
            Merchant? merchant = await _merchantService.GetByUserIdAsync(userId);
            if (merchant == null)
            {
                return NotFound(new { message = "未找到商家信息" });
            }

            Product product = new Product
            {
                ProductId = productId,
                MerchantId = merchant.MerchantId,  // 自动使用当前商家的ID（防止跨商家修改）
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
