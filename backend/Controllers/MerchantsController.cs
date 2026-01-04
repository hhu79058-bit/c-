using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    [ApiController]
    [Route("api/merchants")]
    public class MerchantsController : ControllerBase
    {
        private readonly MerchantService _merchantService;

        public MerchantsController(MerchantService merchantService)
        {
            _merchantService = merchantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var merchants = await _merchantService.GetAllAsync();
            return Ok(merchants);
        }
    }
}
