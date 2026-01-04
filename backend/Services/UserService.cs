using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    public class UserService
    {
        private readonly DbHelper _dbHelper;

        public UserService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<User?> LoginAsync(string userName, string password)
        {
            // TODO: migrate to password hash after data migration.
            string sql = "SELECT * FROM [User] WHERE UserName = @UserName AND Password = @Password";
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserName", userName),
                new SqlParameter("@Password", password)
            };

            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, parameters);
            return dt.Rows.Count > 0 ? MapUser(dt.Rows[0]) : null;
        }

        public async Task<bool> IsUserNameExistsAsync(string userName)
        {
            string sql = "SELECT COUNT(*) FROM [User] WHERE UserName = @UserName";
            object? result = await _dbHelper.ExecuteScalarAsync(sql, new SqlParameter("@UserName", userName));
            return result != null && Convert.ToInt32(result) > 0;
        }

        public async Task<bool> IsPhoneNumberExistsAsync(string phoneNumber)
        {
            string sql = "SELECT COUNT(*) FROM [User] WHERE PhoneNumber = @PhoneNumber";
            object? result = await _dbHelper.ExecuteScalarAsync(sql, new SqlParameter("@PhoneNumber", phoneNumber));
            return result != null && Convert.ToInt32(result) > 0;
        }

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

        public async Task<User?> GetByIdAsync(int userId)
        {
            string sql = "SELECT * FROM [User] WHERE UserID = @UserID";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, new SqlParameter("@UserID", userId));
            return dt.Rows.Count > 0 ? MapUser(dt.Rows[0]) : null;
        }

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
