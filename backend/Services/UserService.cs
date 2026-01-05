using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 用户服务类
    /// 负责用户相关的数据库操作，包括登录、注册、查询等
    /// </summary>
    public class UserService
    {
        /// <summary>
        /// 数据库帮助类实例
        /// </summary>
        private readonly DbHelper _dbHelper;

        /// <summary>
        /// 构造函数：注入数据库帮助类
        /// </summary>
        /// <param name="dbHelper">数据库帮助类</param>
        public UserService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// 用户登录验证
        /// 注意：当前密码为明文比对，建议改为哈希验证
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>登录成功返回用户对象，失败返回null</returns>
        public async Task<User?> LoginAsync(string userName, string password)
        {
            string sql = "SELECT * FROM [User] WHERE UserName = @UserName AND Password = @Password";
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserName", userName),
                new SqlParameter("@Password", password)
            };

            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            return dt.Rows.Count > 0 ? MapUser(dt.Rows[0]) : null;
        }

        /// <summary>
        /// 检查用户名是否已存在
        /// </summary>
        /// <param name="userName">待检查的用户名</param>
        /// <returns>存在返回true，不存在返回false</returns>
        public async Task<bool> IsUserNameExistsAsync(string userName)
        {
            string sql = "SELECT COUNT(*) FROM [User] WHERE UserName = @UserName";
            object? result = await _dbHelper.ExecuteScalarAsync(sql, new SqlParameter("@UserName", userName));
            return result != null && Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// 检查手机号是否已存在
        /// </summary>
        /// <param name="phoneNumber">待检查的手机号</param>
        /// <returns>存在返回true，不存在返回false</returns>
        public async Task<bool> IsPhoneNumberExistsAsync(string phoneNumber)
        {
            string sql = "SELECT COUNT(*) FROM [User] WHERE PhoneNumber = @PhoneNumber";
            object? result = await _dbHelper.ExecuteScalarAsync(sql, new SqlParameter("@PhoneNumber", phoneNumber));
            return result != null && Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// 创建新用户
        /// 注意：密码应在调用前进行哈希处理
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <returns>新创建的用户ID，失败返回0</returns>
        public async Task<int> CreateUserAsync(User user)
        {
            string sql = @"INSERT INTO [User] (UserName, Password, PhoneNumber, Address, UserType)
                           VALUES (@UserName, @Password, @PhoneNumber, @Address, @UserType);
                           SELECT SCOPE_IDENTITY()";
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserName", user.UserName),
                new SqlParameter("@Password", user.Password),
                new SqlParameter("@PhoneNumber", user.PhoneNumber),
                new SqlParameter("@Address", user.Address),
                new SqlParameter("@UserType", user.UserType)
            };

            object? result = await _dbHelper.ExecuteScalarAsync(sql, parameters);
            return result == null ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// 根据用户ID查询用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户对象，不存在返回null</returns>
        public async Task<User?> GetByIdAsync(int userId)
        {
            string sql = "SELECT * FROM [User] WHERE UserID = @UserID";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, new SqlParameter("@UserID", userId));
            return dt.Rows.Count > 0 ? MapUser(dt.Rows[0]) : null;
        }

        /// <summary>
        /// 将数据库行映射为User对象
        /// </summary>
        /// <param name="row">数据库行</param>
        /// <returns>User对象</returns>
        private static User MapUser(DataRow row)
        {
            return new User
            {
                UserId = Convert.ToInt32(row["UserID"]),
                UserName = row["UserName"].ToString() ?? string.Empty,
                Password = row["Password"].ToString() ?? string.Empty,
                PhoneNumber = row["PhoneNumber"].ToString() ?? string.Empty,
                Address = row["Address"].ToString() ?? string.Empty,
                UserType = Convert.ToInt32(row["UserType"])
            };
        }
    }
}
