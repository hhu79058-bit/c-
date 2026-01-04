using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    public class ProductService
    {
        private readonly DbHelper _dbHelper;

        public ProductService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

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

        public async Task<Product?> GetByIdAsync(int productId)
        {
            string sql = "SELECT * FROM Food WHERE FoodID = @FoodID";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql, new SqlParameter("@FoodID", productId));
            return dt.Rows.Count > 0 ? MapProduct(dt.Rows[0]) : null;
        }

        public async Task<int> CreateAsync(Product product)
        {
            string sql = @"INSERT INTO Food (MerchantID, FoodName, Price, Description, IsAvailable)
                           VALUES (@MerchantID, @FoodName, @Price, @Description, @IsAvailable);
                           SELECT SCOPE_IDENTITY()";
            SqlParameter[] parameters =
            {
                new SqlParameter("@MerchantID", product.MerchantId),
                new SqlParameter("@FoodName", product.ProductName),
                new SqlParameter("@Price", product.Price),
                new SqlParameter("@Description", product.Description ?? string.Empty),
                new SqlParameter("@IsAvailable", product.IsAvailable)
            };

            object? result = await _dbHelper.ExecuteScalarAsync(sql, parameters);
            return result == null ? 0 : Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            string sql = @"UPDATE Food SET
                           FoodName = @FoodName,
                           Price = @Price,
                           Description = @Description,
                           IsAvailable = @IsAvailable
                           WHERE FoodID = @FoodID AND MerchantID = @MerchantID";
            SqlParameter[] parameters =
            {
                new SqlParameter("@FoodID", product.ProductId),
                new SqlParameter("@MerchantID", product.MerchantId),
                new SqlParameter("@FoodName", product.ProductName),
                new SqlParameter("@Price", product.Price),
                new SqlParameter("@Description", product.Description ?? string.Empty),
                new SqlParameter("@IsAvailable", product.IsAvailable)
            };

            int rows = await _dbHelper.ExecuteNonQueryAsync(sql, parameters);
            return rows > 0;
        }

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

        private static Product MapProduct(DataRow row)
        {
            return new Product
            {
                ProductId = Convert.ToInt32(row["FoodID"]),
                MerchantId = Convert.ToInt32(row["MerchantID"]),
                ProductName = row["FoodName"].ToString() ?? string.Empty,
                Price = Convert.ToDecimal(row["Price"]),
                Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString(),
                IsAvailable = Convert.ToBoolean(row["IsAvailable"])
            };
        }
    }
}
