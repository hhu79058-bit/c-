using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    public static class RoleHelper
    {
        public static string GetRoleName(int userType)
        {
            return userType switch
            {
                User.TypeAdmin => "Admin",
                User.TypeMerchant => "Merchant",
                _ => "User"
            };
        }
    }
}
