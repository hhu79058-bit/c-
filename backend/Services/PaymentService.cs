using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 支付服务类
    /// 负责处理订单支付相关的业务逻辑
    /// </summary>
    public class PaymentService
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// 构造函数：从配置文件中读取数据库连接字符串
        /// </summary>
        /// <param name="configuration">配置对象</param>
        public PaymentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        /// <summary>
        /// 完成订单支付
        /// 同时更新支付记录表和订单表的支付状态（使用事务保证数据一致性）
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="paymentMethod">支付方式（如：微信、支付宝、现金等）</param>
        /// <returns>支付成功返回true，失败抛出异常</returns>
        public async Task<bool> CompletePaymentAsync(int orderId, string paymentMethod)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // 更新支付记录表：设置为已完成状态并记录支付时间和方式
                string paymentSql = @"UPDATE Payment SET
                    PaymentStatus = @PaymentStatus,
                    PaymentTime = @PaymentTime,
                    PaymentMethod = @PaymentMethod
                    WHERE OrderID = @OrderID";
                SqlCommand paymentCmd = new SqlCommand(paymentSql, connection, transaction);
                paymentCmd.Parameters.AddWithValue("@OrderID", orderId);
                paymentCmd.Parameters.AddWithValue("@PaymentStatus", Payment.StatusCompleted);
                paymentCmd.Parameters.AddWithValue("@PaymentTime", DateTime.Now);
                paymentCmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                await paymentCmd.ExecuteNonQueryAsync();

                // 更新订单表：将订单支付状态设置为已支付
                string orderSql = "UPDATE [Order] SET PayStatus = @PayStatus WHERE OrderID = @OrderID";
                SqlCommand orderCmd = new SqlCommand(orderSql, connection, transaction);
                orderCmd.Parameters.AddWithValue("@PayStatus", Order.PayCompleted);
                orderCmd.Parameters.AddWithValue("@OrderID", orderId);
                await orderCmd.ExecuteNonQueryAsync();

                // 提交事务
                transaction.Commit();
                return true;
            }
            catch
            {
                // 发生异常时回滚事务
                transaction.Rollback();
                throw;
            }
        }
    }
}
