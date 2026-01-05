using System.Data;
using Microsoft.Data.SqlClient;
using WaiMai.Backend.Data;
using WaiMai.Backend.Models;

namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 商品分类服务类
    /// 负责商品分类相关的数据库操作
    /// </summary>
    public class CategoryService
    {
        /// <summary>
        /// 数据库帮助类实例
        /// </summary>
        private readonly DbHelper _dbHelper;

        /// <summary>
        /// 构造函数：注入数据库帮助类
        /// </summary>
        /// <param name="dbHelper">数据库帮助类</param>
        public CategoryService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// 获取所有商品分类
        /// 按分类名称排序
        /// </summary>
        /// <returns>分类列表</returns>
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
