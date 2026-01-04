using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Models;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    [ApiController]
    [Authorize(Roles = "Merchant")]
    [Route("api/merchant/products")]
    public class MerchantProductsController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly MerchantService _merchantService;

        public MerchantProductsController(ProductService productService, MerchantService merchantService)
        {
            _productService = productService;
            _merchantService = merchantService;
        }

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
                MerchantId = merchant.MerchantId,
                ProductName = request.ProductName,
                Price = request.Price,
                Description = request.Description,
                IsAvailable = request.IsAvailable
            };

            int productId = await _productService.CreateAsync(product);
            if (productId <= 0)
            {
                return BadRequest(new { message = "新增失败" });
            }

            return Ok(new { productId });
        }

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
                MerchantId = merchant.MerchantId,
                ProductName = request.ProductName,
                Price = request.Price,
                Description = request.Description,
                IsAvailable = request.IsAvailable
            };

            bool success = await _productService.UpdateAsync(product);
            if (!success)
            {
                return NotFound(new { message = "菜品不存在或更新失败" });
            }

            return Ok(new { message = "更新成功" });
        }

        private int GetUserId()
        {
            string? sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return string.IsNullOrWhiteSpace(sub) ? 0 : Convert.ToInt32(sub);
        }
    }
}
