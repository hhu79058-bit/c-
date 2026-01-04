using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    public class MerchantService
    {
        private readonly DbHelper _dbHelper;

        public MerchantService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<Merchant>> GetAllAsync()
        {
            string sql = "SELECT * FROM Merchant";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql);
            List<Merchant> merchants = new List<Merchant>();
            foreach (DataRow row in dt.Rows)
            {
                merchants.Add(MapMerchant(row));
            }
            return merchants;
        }

        public async Task<Merchant?> GetByUserIdAsync(int userId)
        {
            string sql = "SELECT * FROM Merchant WHERE UserID = @UserID";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, new SqlParameter("@UserID", userId));
            return dt.Rows.Count > 0 ? MapMerchant(dt.Rows[0]) : null;
        }

        private static Merchant MapMerchant(DataRow row)
        {
            return new Merchant
            {
                MerchantId = Convert.ToInt32(row["MerchantID"]),
                UserId = Convert.ToInt32(row["UserID"]),
                ShopName = row["ShopName"].ToString() ?? string.Empty,
                ShopAddress = row["ShopAddress"].ToString() ?? string.Empty,
                ContactPhone = row["ContactPhone"].ToString() ?? string.Empty,
                ShopIntro = row["ShopIntro"] == DBNull.Value ? null : row["ShopIntro"].ToString()
            };
        }
    }
}
