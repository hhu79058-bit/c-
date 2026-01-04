using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    public class OrderService
    {
        private readonly string _connectionString;
        private readonly OrderLogService _orderLogService;

        public OrderService(IConfiguration configuration, OrderLogService orderLogService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            _orderLogService = orderLogService;
        }

        public async Task<int> CreateOrderAsync(int userId, CreateOrderRequest request)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                if (!await UserExistsAsync(connection, transaction, userId))
                {
                    throw new InvalidOperationException("用户不存在");
                }

                if (!await MerchantExistsAsync(connection, transaction, request.MerchantId))
                {
                    throw new InvalidOperationException("商家不存在");
                }

                await ValidateItemsAsync(connection, transaction, request.MerchantId, request.Items);

                string orderNumber = GenerateOrderNumber();
                decimal totalAmount = await CalculateTotalAsync(connection, transaction, request.Items);

                string orderSql = @"INSERT INTO [Order]
                    (OrderNumber, UserID, MerchantID, OrderTime, OrderAmount, OrderStatus, PayStatus, DeliveryAddress)
                    VALUES (@OrderNumber, @UserID, @MerchantID, @OrderTime, @OrderAmount, @OrderStatus, @PayStatus, @DeliveryAddress);
                    SELECT SCOPE_IDENTITY()";

                SqlCommand orderCmd = new SqlCommand(orderSql, connection, transaction);
                orderCmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                orderCmd.Parameters.AddWithValue("@UserID", userId);
                orderCmd.Parameters.AddWithValue("@MerchantID", request.MerchantId);
                orderCmd.Parameters.AddWithValue("@OrderTime", DateTime.Now);
                orderCmd.Parameters.AddWithValue("@OrderAmount", totalAmount);
                orderCmd.Parameters.AddWithValue("@OrderStatus", Order.StatusPending);
                orderCmd.Parameters.AddWithValue("@PayStatus", Order.PayPending);
                orderCmd.Parameters.AddWithValue("@DeliveryAddress", request.DeliveryAddress);

                int orderId = Convert.ToInt32(await orderCmd.ExecuteScalarAsync());

                string detailSql = @"INSERT INTO OrderDetail (OrderID, FoodID, Quantity, Price)
                    VALUES (@OrderID, @FoodID, @Quantity, @Price)";

                foreach (CreateOrderItemDto item in request.Items)
                {
                    decimal price = await GetProductPriceAsync(connection, transaction, item.ProductId);
                    SqlCommand detailCmd = new SqlCommand(detailSql, connection, transaction);
                    detailCmd.Parameters.AddWithValue("@OrderID", orderId);
                    detailCmd.Parameters.AddWithValue("@FoodID", item.ProductId);
                    detailCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    detailCmd.Parameters.AddWithValue("@Price", price);
                    await detailCmd.ExecuteNonQueryAsync();
                }

                string paymentSql = @"INSERT INTO Payment (OrderID, PaymentAmount, PaymentStatus)
                    VALUES (@OrderID, @PaymentAmount, @PaymentStatus)";

                SqlCommand paymentCmd = new SqlCommand(paymentSql, connection, transaction);
                paymentCmd.Parameters.AddWithValue("@OrderID", orderId);
                paymentCmd.Parameters.AddWithValue("@PaymentAmount", totalAmount);
                paymentCmd.Parameters.AddWithValue("@PaymentStatus", Payment.StatusPending);
                await paymentCmd.ExecuteNonQueryAsync();

                transaction.Commit();
                await _orderLogService.CreateAsync(new OrderLog
                {
                    OrderId = orderId,
                    FromStatus = Order.StatusPending,
                    ToStatus = Order.StatusPending,
                    ChangedAt = DateTime.Now,
                    Remark = "订单创建"
                });
                return orderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<Order>> GetOrdersByUserAsync(int userId)
        {
            string sql = @"SELECT * FROM [Order]
                           WHERE UserID = @UserID
                           ORDER BY OrderTime DESC";

            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UserID", userId);

            using SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            List<Order> orders = new List<Order>();
            foreach (DataRow row in table.Rows)
            {
                orders.Add(MapOrder(row));
            }
            return orders;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            string sql = "SELECT * FROM [Order] ORDER BY OrderTime DESC";
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            List<Order> orders = new List<Order>();
            foreach (DataRow row in table.Rows)
            {
                orders.Add(MapOrder(row));
            }
            return orders;
        }

        public async Task<List<Order>> GetOrdersByMerchantAsync(int merchantId)
        {
            string sql = @"SELECT * FROM [Order]
                           WHERE MerchantID = @MerchantID
                           ORDER BY OrderTime DESC";

            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@MerchantID", merchantId);

            using SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            List<Order> orders = new List<Order>();
            foreach (DataRow row in table.Rows)
            {
                orders.Add(MapOrder(row));
            }
            return orders;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, int orderStatus)
        {
            int? currentStatus = await GetOrderStatusAsync(orderId);
            if (currentStatus == null)
            {
                return false;
            }

            string sql = "UPDATE [Order] SET OrderStatus = @OrderStatus WHERE OrderID = @OrderID";
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderStatus", orderStatus);
            command.Parameters.AddWithValue("@OrderID", orderId);
            int rows = await command.ExecuteNonQueryAsync();
            if (rows > 0)
            {
                await _orderLogService.CreateAsync(new OrderLog
                {
                    OrderId = orderId,
                    FromStatus = currentStatus.Value,
                    ToStatus = orderStatus,
                    ChangedAt = DateTime.Now,
                    Remark = "后台更新订单状态"
                });
                return true;
            }
            return false;
        }

        private static Order MapOrder(DataRow row)
        {
            return new Order
            {
                OrderId = Convert.ToInt32(row["OrderID"]),
                OrderNumber = row["OrderNumber"].ToString() ?? string.Empty,
                UserId = Convert.ToInt32(row["UserID"]),
                MerchantId = Convert.ToInt32(row["MerchantID"]),
                OrderTime = Convert.ToDateTime(row["OrderTime"]),
                OrderAmount = Convert.ToDecimal(row["OrderAmount"]),
                OrderStatus = Convert.ToInt32(row["OrderStatus"]),
                PayStatus = Convert.ToInt32(row["PayStatus"]),
                DeliveryAddress = row["DeliveryAddress"].ToString() ?? string.Empty
            };
        }

        private async Task<decimal> CalculateTotalAsync(SqlConnection connection, SqlTransaction transaction, List<CreateOrderItemDto> items)
        {
            decimal total = 0;
            foreach (CreateOrderItemDto item in items)
            {
                decimal price = await GetProductPriceAsync(connection, transaction, item.ProductId);
                total += price * item.Quantity;
            }
            return total;
        }

        private static async Task<decimal> GetProductPriceAsync(SqlConnection connection, SqlTransaction transaction, int productId)
        {
            string sql = "SELECT Price FROM Food WHERE FoodID = @FoodID";
            SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@FoodID", productId);
            object? result = await command.ExecuteScalarAsync();
            return result == null ? 0 : Convert.ToDecimal(result);
        }

        private static async Task<bool> UserExistsAsync(SqlConnection connection, SqlTransaction transaction, int userId)
        {
            string sql = "SELECT COUNT(*) FROM [User] WHERE UserID = @UserID";
            SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@UserID", userId);
            object? result = await command.ExecuteScalarAsync();
            return result != null && Convert.ToInt32(result) > 0;
        }

        private static async Task<bool> MerchantExistsAsync(SqlConnection connection, SqlTransaction transaction, int merchantId)
        {
            string sql = "SELECT COUNT(*) FROM Merchant WHERE MerchantID = @MerchantID";
            SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@MerchantID", merchantId);
            object? result = await command.ExecuteScalarAsync();
            return result != null && Convert.ToInt32(result) > 0;
        }

        private static async Task ValidateItemsAsync(SqlConnection connection, SqlTransaction transaction, int merchantId, List<CreateOrderItemDto> items)
        {
            foreach (CreateOrderItemDto item in items)
            {
                string sql = "SELECT MerchantID FROM Food WHERE FoodID = @FoodID";
                SqlCommand command = new SqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@FoodID", item.ProductId);
                object? result = await command.ExecuteScalarAsync();
                if (result == null)
                {
                    throw new InvalidOperationException("菜品不存在");
                }

                int productMerchantId = Convert.ToInt32(result);
                if (productMerchantId != merchantId)
                {
                    throw new InvalidOperationException("菜品与商家不匹配");
                }
            }
        }

        private async Task<int?> GetOrderStatusAsync(int orderId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlCommand command = new SqlCommand("SELECT OrderStatus FROM [Order] WHERE OrderID = @OrderID", connection);
            command.Parameters.AddWithValue("@OrderID", orderId);
            object? result = await command.ExecuteScalarAsync();
            return result == null ? null : Convert.ToInt32(result);
        }

        private static string GenerateOrderNumber()
        {
            Random random = new Random();
            return DateTime.Now.ToString("yyyyMMddHHmmss") + random.Next(10000, 99999);
        }
    }
}
