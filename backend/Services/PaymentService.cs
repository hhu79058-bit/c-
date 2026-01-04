using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    public class PaymentService
    {
        private readonly string _connectionString;

        public PaymentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public async Task<bool> CompletePaymentAsync(int orderId, string paymentMethod)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
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

                string orderSql = "UPDATE [Order] SET PayStatus = @PayStatus WHERE OrderID = @OrderID";
                SqlCommand orderCmd = new SqlCommand(orderSql, connection, transaction);
                orderCmd.Parameters.AddWithValue("@PayStatus", Order.PayCompleted);
                orderCmd.Parameters.AddWithValue("@OrderID", orderId);
                await orderCmd.ExecuteNonQueryAsync();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
