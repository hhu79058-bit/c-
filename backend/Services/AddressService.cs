using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    public class AddressService
    {
        private readonly DbHelper _dbHelper;

        public AddressService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

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
