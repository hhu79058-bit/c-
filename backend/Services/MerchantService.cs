using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 商家服务类
    /// 负责商家相关的数据库操作
    /// </summary>
    public class MerchantService
    {
        /// <summary>
        /// 数据库帮助类实例
        /// </summary>
        private readonly DbHelper _dbHelper;

        /// <summary>
        /// 构造函数：注入数据库帮助类
        /// </summary>
        /// <param name="dbHelper">数据库帮助类</param>
        public MerchantService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// 获取所有商家列表
        /// </summary>
        /// <returns>商家列表</returns>
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

        /// <summary>
        /// 根据用户ID查询商家信息
        /// 用于商家登录后获取其店铺信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>商家对象，不存在返回null</returns>
        public async Task<Merchant?> GetByUserIdAsync(int userId)
        {
            string sql = "SELECT * FROM Merchant WHERE UserID = @UserID";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, new SqlParameter("@UserID", userId));
            return dt.Rows.Count > 0 ? MapMerchant(dt.Rows[0]) : null;
        }

        /// <summary>
        /// 将数据库行映射为Merchant对象
        /// </summary>
        /// <param name="row">数据库行</param>
        /// <returns>Merchant对象</returns>
        private static Merchant MapMerchant(DataRow row)
        {
            return new Merchant
            {
                MerchantId = Convert.ToInt32(row["MerchantID"]),
                UserId = Convert.ToInt32(row["UserID"]),
                ShopName = row["ShopName"].ToString() ?? string.Empty,
                ShopAddress = row["ShopAddress"].ToString() ?? string.Empty,
                ContactPhone = row["ContactPhone"].ToString() ?? string.Empty,
                ShopIntro = row["ShopIntro"] == DBNull.Value ? null : row["ShopIntro"].ToString(),
                LogoUrl = row["LogoUrl"] == DBNull.Value ? null : row["LogoUrl"].ToString()
            };
        }
    }
}
