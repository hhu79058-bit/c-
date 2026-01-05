using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 订单日志服务类
    /// 负责订单状态变更日志的记录和查询
    /// </summary>
    public class OrderLogService
    {
        /// <summary>
        /// 数据库帮助类实例
        /// </summary>
        private readonly DbHelper _dbHelper;

        /// <summary>
        /// 构造函数：注入数据库帮助类
        /// </summary>
        /// <param name="dbHelper">数据库帮助类</param>
        public OrderLogService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// 创建订单状态变更日志
        /// </summary>
        /// <param name="log">订单日志对象</param>
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

        /// <summary>
        /// 根据订单ID查询该订单的所有状态变更日志
        /// 按变更时间倒序排列（最新的在前）
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>订单日志列表</returns>
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
