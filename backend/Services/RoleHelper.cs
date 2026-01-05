using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 角色辅助类
    /// 用于将用户类型整数值转换为角色名称字符串
    /// 用于JWT Claims和授权策略
    /// </summary>
    public static class RoleHelper
    {
        /// <summary>
        /// 根据用户类型获取对应的角色名称
        /// </summary>
        /// <param name="userType">用户类型（0-顾客，1-商家，2-管理员）</param>
        /// <returns>角色名称字符串（"User"、"Merchant"、"Admin"）</returns>
        public static string GetRoleName(int userType)
        {
            return userType switch
            {
                User.TypeAdmin => "Admin",        // 管理员
                User.TypeMerchant => "Merchant",  // 商家
                _ => "User"                        // 普通用户（顾客）
            };
        }
    }
}
