using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 收货地址服务类
    /// 负责用户收货地址相关的数据库操作
    /// </summary>
    public class AddressService
    {
        /// <summary>
        /// 数据库帮助类实例
        /// </summary>
        private readonly DbHelper _dbHelper;

        /// <summary>
        /// 构造函数：注入数据库帮助类
        /// </summary>
        /// <param name="dbHelper">数据库帮助类</param>
        public AddressService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// 根据用户ID查询该用户的所有收货地址
        /// 按默认地址优先、ID倒序排列
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>地址列表</returns>
        public async Task<List<Address>> GetByUserIdAsync(int userId)
        {
            string sql = "SELECT * FROM Address WHERE UserID = @UserID ORDER BY IsDefault DESC, AddressID DESC";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, new SqlParameter("@UserID", userId));
            List<Address> addresses = new List<Address>();
            foreach (DataRow row in dt.Rows)
            {
                addresses.Add(MapAddress(row));
            }
            return addresses;
        }

        /// <summary>
        /// 创建新收货地址
        /// </summary>
        /// <param name="address">地址对象</param>
        /// <returns>新创建的地址ID，失败返回0</returns>
        public async Task<int> CreateAsync(Address address)
        {
            string sql = @"INSERT INTO Address (UserID, RecipientName, PhoneNumber, FullAddress, IsDefault)
                           VALUES (@UserID, @RecipientName, @PhoneNumber, @FullAddress, @IsDefault);
                           SELECT SCOPE_IDENTITY()";
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserID", address.UserId),
                new SqlParameter("@RecipientName", address.RecipientName),
                new SqlParameter("@PhoneNumber", address.PhoneNumber),
                new SqlParameter("@FullAddress", address.FullAddress),
                new SqlParameter("@IsDefault", address.IsDefault)
            };

            object? result = await _dbHelper.ExecuteScalarAsync(sql, parameters);
            return result == null ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// 将数据库行映射为Address对象
        /// </summary>
        /// <param name="row">数据库行</param>
        /// <returns>Address对象</returns>
        private static Address MapAddress(DataRow row)
        {
            return new Address
            {
                AddressId = Convert.ToInt32(row["AddressID"]),
                UserId = Convert.ToInt32(row["UserID"]),
                RecipientName = row["RecipientName"].ToString() ?? string.Empty,
                PhoneNumber = row["PhoneNumber"].ToString() ?? string.Empty,
                FullAddress = row["FullAddress"].ToString() ?? string.Empty,
                IsDefault = Convert.ToBoolean(row["IsDefault"])
            };
        }
    }
}
