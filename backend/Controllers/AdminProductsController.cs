using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Models;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/products")]
    public class AdminProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public AdminProductsController(ProductService productService)
        {
            _productService = productService;
        }

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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdminCreateProductRequest request)
        {
            Product product = new Product
            {
                MerchantId = request.MerchantId,
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
            Product product = new Product
            {
                ProductId = productId,
                MerchantId = request.MerchantId,
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
