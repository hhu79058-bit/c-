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
    [Authorize]
    [Route("api/addresses")]
    public class AddressesController : ControllerBase
    {
        private readonly AddressService _addressService;

        public AddressesController(AddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            int userId = GetUserId();
            var addresses = await _addressService.GetByUserIdAsync(userId);
            return Ok(addresses);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddressRequest request)
        {
            int userId = GetUserId();
            Address address = new Address
            {
                UserId = userId,
                RecipientName = request.RecipientName,
                PhoneNumber = request.PhoneNumber,
                FullAddress = request.FullAddress,
                IsDefault = request.IsDefault
            };

            int addressId = await _addressService.CreateAsync(address);
            if (addressId <= 0)
            {
                return BadRequest(new { message = "地址新增失败" });
            }

            return Ok(new { addressId });
        }

        private int GetUserId()
        {
            string? sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return string.IsNullOrWhiteSpace(sub) ? 0 : Convert.ToInt32(sub);
        }
    }
}
