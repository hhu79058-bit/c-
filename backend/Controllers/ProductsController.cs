using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
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
    }
}
