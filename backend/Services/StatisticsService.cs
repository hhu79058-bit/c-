using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 统计服务类
    /// 负责生成系统各类统计数据（如订单总数、营业额等）
    /// </summary>
    public class StatisticsService
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// 构造函数：从配置文件中读取数据库连接字符串
        /// </summary>
        /// <param name="configuration">配置对象</param>
        public StatisticsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        /// <summary>
        /// 获取订单统计汇总信息
        /// 包括：订单总数、总营业额、今日订单数
        /// </summary>
        /// <returns>统计数据字典</returns>
        public async Task<Dictionary<string, object>> GetSummaryAsync()
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // 执行统计SQL查询
            string sql = @"SELECT
                COUNT(*) AS TotalOrders,
                SUM(OrderAmount) AS TotalRevenue,
                SUM(CASE WHEN CAST(OrderTime AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS TodayOrders
                FROM [Order]";

            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            // 如果没有数据，返回空字典
            if (table.Rows.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            // 构造返回结果
            DataRow row = table.Rows[0];
            return new Dictionary<string, object>
            {
                { "totalOrders", row["TotalOrders"] == DBNull.Value ? 0 : Convert.ToInt32(row["TotalOrders"]) },
                { "totalRevenue", row["TotalRevenue"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalRevenue"]) },
                { "todayOrders", row["TodayOrders"] == DBNull.Value ? 0 : Convert.ToInt32(row["TodayOrders"]) }
            };
        }
    }
}
