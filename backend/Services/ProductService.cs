using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 商品（菜品）服务类
    /// 负责商品相关的数据库操作，包括查询、创建、更新、上下架等
    /// </summary>
    public class ProductService
    {
        /// <summary>
        /// 数据库帮助类实例
        /// </summary>
        private readonly DbHelper _dbHelper;

        /// <summary>
        /// 构造函数：注入数据库帮助类
        /// </summary>
        /// <param name="dbHelper">数据库帮助类</param>
        public ProductService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// 根据商家ID查询该商家的所有商品
        /// </summary>
        /// <param name="merchantId">商家ID</param>
        /// <returns>商品列表</returns>
        public async Task<List<Product>> GetByMerchantIdAsync(int merchantId)
        {
            string sql = "SELECT * FROM Food WHERE MerchantID = @MerchantID";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, new SqlParameter("@MerchantID", merchantId));
            List<Product> products = new List<Product>();
            foreach (DataRow row in dt.Rows)
            {
                products.Add(MapProduct(row));
            }
            return products;
        }

        /// <summary>
        /// 根据商品ID查询单个商品详情
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>商品对象，不存在返回null</returns>
        public async Task<Product?> GetByIdAsync(int productId)
        {
            string sql = "SELECT * FROM Food WHERE FoodID = @FoodID";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, new SqlParameter("@FoodID", productId));
            return dt.Rows.Count > 0 ? MapProduct(dt.Rows[0]) : null;
        }

        /// <summary>
        /// 创建新商品
        /// </summary>
        /// <param name="product">商品对象</param>
        /// <returns>新创建的商品ID，失败返回0</returns>
        public async Task<int> CreateAsync(Product product)
        {
            string sql = @"INSERT INTO Food (MerchantID, FoodName, Price, Description, IsAvailable, ImageUrl)
                           VALUES (@MerchantID, @FoodName, @Price, @Description, @IsAvailable, @ImageUrl);
                           SELECT SCOPE_IDENTITY()";
            SqlParameter[] parameters =
            {
                new SqlParameter("@MerchantID", product.MerchantId),
                new SqlParameter("@FoodName", product.ProductName),
                new SqlParameter("@Price", product.Price),
                new SqlParameter("@Description", product.Description ?? string.Empty),
                new SqlParameter("@IsAvailable", product.IsAvailable),
                new SqlParameter("@ImageUrl", product.ImageUrl ?? (object)DBNull.Value)
            };

            object? result = await _dbHelper.ExecuteScalarAsync(sql, parameters);
            return result == null ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// 更新商品信息
        /// 包含商家ID校验，防止跨商家修改商品
        /// </summary>
        /// <param name="product">商品对象</param>
        /// <returns>更新成功返回true，失败返回false</returns>
        public async Task<bool> UpdateAsync(Product product)
        {
            string sql = @"UPDATE Food SET
                           FoodName = @FoodName,
                           Price = @Price,
                           Description = @Description,
                           IsAvailable = @IsAvailable,
                           ImageUrl = @ImageUrl
                           WHERE FoodID = @FoodID AND MerchantID = @MerchantID";
            SqlParameter[] parameters =
            {
                new SqlParameter("@FoodID", product.ProductId),
                new SqlParameter("@MerchantID", product.MerchantId),
                new SqlParameter("@FoodName", product.ProductName),
                new SqlParameter("@Price", product.Price),
                new SqlParameter("@Description", product.Description ?? string.Empty),
                new SqlParameter("@IsAvailable", product.IsAvailable),
                new SqlParameter("@ImageUrl", product.ImageUrl ?? (object)DBNull.Value)
            };

            int rows = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rows > 0;
        }

        /// <summary>
        /// 切换商品上架/下架状态
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="merchantId">商家ID（用于权限验证）</param>
        /// <param name="isAvailable">目标状态（true-上架，false-下架）</param>
        /// <returns>切换成功返回true，失败返回false</returns>
        public async Task<bool> ToggleAvailabilityAsync(int productId, int merchantId, bool isAvailable)
        {
            string sql = "UPDATE Food SET IsAvailable = @IsAvailable WHERE FoodID = @FoodID AND MerchantID = @MerchantID";
            SqlParameter[] parameters =
            {
                new SqlParameter("@FoodID", productId),
                new SqlParameter("@MerchantID", merchantId),
                new SqlParameter("@IsAvailable", isAvailable)
            };

            int rows = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rows > 0;
        }

        /// <summary>
        /// 将数据库行映射为Product对象
        /// </summary>
        /// <param name="row">数据库行</param>
        /// <returns>Product对象</returns>
        private static Product MapProduct(DataRow row)
        {
            return new Product
            {
                ProductId = Convert.ToInt32(row["FoodID"]),
                MerchantId = Convert.ToInt32(row["MerchantID"]),
                ProductName = row["FoodName"].ToString() ?? string.Empty,
                Price = Convert.ToDecimal(row["Price"]),
                Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString(),
                IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                ImageUrl = row["ImageUrl"] == DBNull.Value ? null : row["ImageUrl"].ToString()
            };
        }
    }
}
