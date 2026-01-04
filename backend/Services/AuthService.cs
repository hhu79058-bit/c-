using WaiMai.Backend.DTOs;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly JwtTokenService _tokenService;

        public AuthService(UserService userService, JwtTokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            User? user = await _userService.LoginAsync(request.UserName, request.Password);
            if (user == null)
            {
                return null;
            }

            return new AuthResponse
            {
                Token = _tokenService.CreateToken(user),
                UserId = user.UserId,
                UserName = user.UserName,
                UserType = user.UserType
            };
        }

        public async Task<(bool Success, string Message, AuthResponse? Response)> RegisterAsync(RegisterRequest request)
        {
            bool nameExists = await _userService.IsUserNameExistsAsync(request.UserName);
            if (nameExists)
            {
                return (false, "用户名已存在", null);
            }

            bool phoneExists = await _userService.IsPhoneNumberExistsAsync(request.PhoneNumber);
            if (phoneExists)
            {
                return (false, "手机号已存在", null);
            }

            User user = new User
            {
                UserName = request.UserName,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                UserType = request.UserType
            };

            int userId = await _userService.CreateUserAsync(user);
            if (userId <= 0)
            {
                return (false, "注册失败", null);
            }

            user.UserId = userId;
            return (true, "注册成功", new AuthResponse
            {
                Token = _tokenService.CreateToken(user),
                UserId = user.UserId,
                UserName = user.UserName,
                UserType = user.UserType
            });
        }
    }
}
