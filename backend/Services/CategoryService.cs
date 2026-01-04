using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    public class CategoryService
    {
        private readonly DbHelper _dbHelper;

        public CategoryService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            string sql = "SELECT * FROM Category ORDER BY CategoryName";
            DataTable dt = await _dbHelper.ExecuteQueryAsync(sql);
            List<Category> categories = new List<Category>();
            foreach (DataRow row in dt.Rows)
            {
                categories.Add(new Category
                {
                    CategoryId = Convert.ToInt32(row["CategoryID"]),
                    CategoryName = row["CategoryName"].ToString() ?? string.Empty
                });
            }
            return categories;
        }
    }
}
