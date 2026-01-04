using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    public class OrderLogService
    {
        private readonly DbHelper _dbHelper;

        public OrderLogService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task CreateAsync(OrderLog log)
        {
            string sql = @"INSERT INTO OrderLog (OrderID, FromStatus, ToStatus, ChangedAt, Remark)
                           VALUES (@OrderID, @FromStatus, @ToStatus, @ChangedAt, @Remark)";
            SqlParameter[] parameters =
            {
                new SqlParameter("@OrderID", log.OrderId),
                new SqlParameter("@FromStatus", log.FromStatus),
                new SqlParameter("@ToStatus", log.ToStatus),
                new SqlParameter("@ChangedAt", log.ChangedAt),
                new SqlParameter("@Remark", log.Remark ?? string.Empty)
            };

            await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<List<OrderLog>> GetByOrderIdAsync(int orderId)
        {
            string sql = "SELECT * FROM OrderLog WHERE OrderID = @OrderID ORDER BY ChangedAt DESC";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, new SqlParameter("@OrderID", orderId));
            List<OrderLog> logs = new List<OrderLog>();
            foreach (DataRow row in dt.Rows)
            {
                logs.Add(new OrderLog
                {
                    OrderLogId = Convert.ToInt32(row["OrderLogID"]),
                    OrderId = Convert.ToInt32(row["OrderID"]),
                    FromStatus = Convert.ToInt32(row["FromStatus"]),
                    ToStatus = Convert.ToInt32(row["ToStatus"]),
                    ChangedAt = Convert.ToDateTime(row["ChangedAt"]),
                    Remark = row["Remark"] == DBNull.Value ? null : row["Remark"].ToString()
                });
            }
            return logs;
        }
    }
}
