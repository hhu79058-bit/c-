using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace WaiMai.Backend.Services
{
    public class StatisticsService
    {
        private readonly string _connectionString;

        public StatisticsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public async Task<Dictionary<string, object>> GetSummaryAsync()
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"SELECT
                COUNT(*) AS TotalOrders,
                SUM(OrderAmount) AS TotalRevenue,
                SUM(CASE WHEN CAST(OrderTime AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS TodayOrders
                FROM [Order]";

            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            if (table.Rows.Count == 0)
            {
                return new Dictionary<string, object>();
            }

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
