using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 订单服务类
    /// 负责订单相关的所有业务逻辑，包括创建订单、查询订单、更新状态等
    /// 这是系统核心服务之一
    /// </summary>
    public class OrderService
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// 构造函数：从配置文件中读取数据库连接字符串
        /// </summary>
        /// <param name="configuration">配置对象</param>
        public OrderService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        /// <summary>
        /// 创建新订单
        /// 使用事务确保订单、订单明细、支付记录和日志的原子性
        /// </summary>
        /// <param name="userId">下单用户ID</param>
        /// <param name="request">订单请求数据</param>
        /// <returns>新创建的订单ID</returns>
        /// <exception cref="InvalidOperationException">用户不存在、商家不存在、商品验证失败时抛出</exception>
        public async Task<int> CreateOrderAsync(int userId, CreateOrderRequest request)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // 1. 验证用户是否存在
                if (!await UserExistsAsync(connection, transaction, userId))
                {
                    throw new InvalidOperationException("用户不存在");
                }

                // 2. 验证商家是否存在
                if (!await MerchantExistsAsync(connection, transaction, request.MerchantId))
                {
                    throw new InvalidOperationException("商家不存在");
                }

                // 3. 验证订单项（检查商品是否存在且属于该商家）
                await ValidateItemsAsync(connection, transaction, request.MerchantId, request.Items);

                // 4. 生成订单号和计算总金额
                string orderNumber = GenerateOrderNumber();
                decimal totalAmount = await CalculateTotalAsync(connection, transaction, request.Items);

                // 5. 插入订单主表
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

                // 6. 插入订单明细表（循环插入每个商品项）
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

                // 7. 创建支付记录（初始状态为待支付）
                string paymentSql = @"INSERT INTO Payment (OrderID, PaymentAmount, PaymentStatus)
                    VALUES (@OrderID, @PaymentAmount, @PaymentStatus)";

                SqlCommand paymentCmd = new SqlCommand(paymentSql, connection, transaction);
                paymentCmd.Parameters.AddWithValue("@OrderID", orderId);
                paymentCmd.Parameters.AddWithValue("@PaymentAmount", totalAmount);
                paymentCmd.Parameters.AddWithValue("@PaymentStatus", Payment.StatusPending);
                await paymentCmd.ExecuteNonQueryAsync();

                // 8. 记录订单日志
                await CreateOrderLogAsync(connection, transaction, new OrderLog
                {
                    OrderId = orderId,
                    FromStatus = Order.StatusPending,
                    ToStatus = Order.StatusPending,
                    ChangedAt = DateTime.Now,
                    Remark = "订单创建"
                });

                // 9. 提交事务
                transaction.Commit();
                return orderId;
            }
            catch
            {
                // 发生异常时回滚事务
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 查询指定用户的所有订单
        /// 按下单时间倒序排列
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>订单列表</returns>
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

        /// <summary>
        /// 查询所有订单（管理员使用）
        /// 按下单时间倒序排列
        /// </summary>
        /// <returns>订单列表</returns>
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

        /// <summary>
        /// 查询指定商家的所有订单
        /// 按下单时间倒序排列
        /// </summary>
        /// <param name="merchantId">商家ID</param>
        /// <returns>订单列表</returns>
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

        /// <summary>
        /// 更新订单状态
        /// 使用事务同时记录状态变更日志
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="orderStatus">新的订单状态</param>
        /// <returns>更新成功返回true，订单不存在返回false</returns>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, int orderStatus)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // 查询当前订单状态
                int? currentStatus = await GetOrderStatusAsync(connection, transaction, orderId);
                if (currentStatus == null)
                {
                    return false;  // 订单不存在
                }

                // 更新订单状态
                string sql = "UPDATE [Order] SET OrderStatus = @OrderStatus WHERE OrderID = @OrderID";
                using SqlCommand command = new SqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@OrderStatus", orderStatus);
                command.Parameters.AddWithValue("@OrderID", orderId);
                int rows = await command.ExecuteNonQueryAsync();
                if (rows <= 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // 记录状态变更日志
                await CreateOrderLogAsync(connection, transaction, new OrderLog
                {
                    OrderId = orderId,
                    FromStatus = currentStatus.Value,
                    ToStatus = orderStatus,
                    ChangedAt = DateTime.Now,
                    Remark = "后台更新订单状态"
                });

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 将数据库行映射为Order对象
        /// </summary>
        /// <param name="row">数据库行</param>
        /// <returns>Order对象</returns>
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

        /// <summary>
        /// 计算订单总金额
        /// 遍历所有订单项，累加（单价 × 数量）
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务对象</param>
        /// <param name="items">订单项列表</param>
        /// <returns>订单总金额</returns>
        /// <exception cref="InvalidOperationException">商品数量小于等于0时抛出</exception>
        private async Task<decimal> CalculateTotalAsync(SqlConnection connection, SqlTransaction transaction, List<CreateOrderItemDto> items)
        {
            decimal total = 0;
            foreach (CreateOrderItemDto item in items)
            {
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException("菜品数量必须大于 0");
                }
                decimal price = await GetProductPriceAsync(connection, transaction, item.ProductId);
                total += price * item.Quantity;
            }
            return total;
        }

        /// <summary>
        /// 获取商品当前价格（从数据库查询）
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务对象</param>
        /// <param name="productId">商品ID</param>
        /// <returns>商品价格，不存在返回0</returns>
        private static async Task<decimal> GetProductPriceAsync(SqlConnection connection, SqlTransaction transaction, int productId)
        {
            string sql = "SELECT Price FROM Food WHERE FoodID = @FoodID";
            SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@FoodID", productId);
            object? result = await command.ExecuteScalarAsync();
            return result == null ? 0 : Convert.ToDecimal(result);
        }

        /// <summary>
        /// 检查用户是否存在
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务对象</param>
        /// <param name="userId">用户ID</param>
        /// <returns>存在返回true，不存在返回false</returns>
        private static async Task<bool> UserExistsAsync(SqlConnection connection, SqlTransaction transaction, int userId)
        {
            string sql = "SELECT COUNT(*) FROM [User] WHERE UserID = @UserID";
            SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@UserID", userId);
            object? result = await command.ExecuteScalarAsync();
            return result != null && Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// 检查商家是否存在
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务对象</param>
        /// <param name="merchantId">商家ID</param>
        /// <returns>存在返回true，不存在返回false</returns>
        private static async Task<bool> MerchantExistsAsync(SqlConnection connection, SqlTransaction transaction, int merchantId)
        {
            string sql = "SELECT COUNT(*) FROM Merchant WHERE MerchantID = @MerchantID";
            SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@MerchantID", merchantId);
            object? result = await command.ExecuteScalarAsync();
            return result != null && Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// 验证订单项（检查商品是否存在且属于指定商家）
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务对象</param>
        /// <param name="merchantId">商家ID</param>
        /// <param name="items">订单项列表</param>
        /// <exception cref="InvalidOperationException">商品不存在或商品与商家不匹配时抛出</exception>
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

        /// <summary>
        /// 获取订单所属的商家ID
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>商家ID，订单不存在返回null</returns>
        public async Task<int?> GetOrderMerchantIdAsync(int orderId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlCommand command = new SqlCommand("SELECT MerchantID FROM [Order] WHERE OrderID = @OrderID", connection);
            command.Parameters.AddWithValue("@OrderID", orderId);
            object? result = await command.ExecuteScalarAsync();
            return result == null ? null : Convert.ToInt32(result);
        }

        /// <summary>
        /// 获取订单当前状态
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务对象</param>
        /// <param name="orderId">订单ID</param>
        /// <returns>订单状态，订单不存在返回null</returns>
        private static async Task<int?> GetOrderStatusAsync(SqlConnection connection, SqlTransaction transaction, int orderId)
        {
            using SqlCommand command = new SqlCommand("SELECT OrderStatus FROM [Order] WHERE OrderID = @OrderID", connection, transaction);
            command.Parameters.AddWithValue("@OrderID", orderId);
            object? result = await command.ExecuteScalarAsync();
            return result == null ? null : Convert.ToInt32(result);
        }

        /// <summary>
        /// 创建订单状态变更日志（在事务中）
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务对象</param>
        /// <param name="log">订单日志对象</param>
        private static async Task CreateOrderLogAsync(SqlConnection connection, SqlTransaction transaction, OrderLog log)
        {
            string sql = @"INSERT INTO OrderLog (OrderID, FromStatus, ToStatus, ChangedAt, Remark)
                           VALUES (@OrderID, @FromStatus, @ToStatus, @ChangedAt, @Remark)";
            using SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@OrderID", log.OrderId);
            command.Parameters.AddWithValue("@FromStatus", log.FromStatus);
            command.Parameters.AddWithValue("@ToStatus", log.ToStatus);
            command.Parameters.AddWithValue("@ChangedAt", log.ChangedAt);
            command.Parameters.AddWithValue("@Remark", log.Remark ?? string.Empty);
            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 生成唯一订单号
        /// 格式：yyyyMMddHHmmss + 5位随机数（10000-99999）
        /// 注意：高并发场景下可能重复，建议改用GUID或数据库序列
        /// </summary>
        /// <returns>订单编号</returns>
        private static string GenerateOrderNumber()
        {
            Random random = new Random();
            return DateTime.Now.ToString("yyyyMMddHHmmss") + random.Next(10000, 99999);
        }

        /// <summary>
        /// 根据订单ID获取订单
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>订单对象，不存在返回null</returns>
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlCommand command = new SqlCommand("SELECT * FROM [Order] WHERE OrderID = @OrderID", connection);
            command.Parameters.AddWithValue("@OrderID", orderId);
            using SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);
            if (table.Rows.Count == 0) return null;
            return MapOrder(table.Rows[0]);
        }
    }
}
