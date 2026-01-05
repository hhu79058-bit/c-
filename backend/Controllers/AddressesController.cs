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
    /// 收货地址管理控制器
    /// 需要登录，用户管理自己的收货地址
    /// </summary>
    [ApiController]
    [Authorize]  // 要求登录
    [Route("api/addresses")]
    public class AddressesController : ControllerBase
    {
        private readonly AddressService _addressService;

        /// <summary>
        /// 构造函数：注入地址服务
        /// </summary>
        /// <param name="addressService">地址服务</param>
        public AddressesController(AddressService addressService)
        {
            _addressService = addressService;
        }

        /// <summary>
        /// 获取当前用户的所有收货地址
        /// GET /api/addresses
        /// </summary>
        /// <returns>地址列表（默认地址优先）</returns>
        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            int userId = GetUserId();
            var addresses = await _addressService.GetByUserIdAsync(userId);
            return Ok(addresses);
        }

        /// <summary>
        /// 创建新收货地址
        /// POST /api/addresses
        /// </summary>
        /// <param name="request">地址创建请求</param>
        /// <returns>成功返回200和地址ID，失败返回400</returns>
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
