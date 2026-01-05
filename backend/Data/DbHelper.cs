using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace WaiMai.Backend.Data
{
    /// <summary>
    /// 数据库帮助类
    /// 封装了常用的数据库操作，简化Services层的数据访问代码
    /// 使用ADO.NET和SqlClient进行数据库交互
    /// </summary>
    public class DbHelper
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// 构造函数：从配置文件中读取数据库连接字符串
        /// </summary>
        /// <param name="configuration">配置对象（注入）</param>
        public DbHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        /// <summary>
        /// 执行查询操作（SELECT），返回DataTable
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">SQL参数数组（防止SQL注入）</param>
        /// <returns>查询结果DataTable</returns>
        public async Task<DataTable> ExecuteQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlCommand command = new SqlCommand(sql, connection);
            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            using SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        /// <summary>
        /// 执行非查询操作（INSERT、UPDATE、DELETE）
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数数组（防止SQL注入）</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlCommand command = new SqlCommand(sql, connection);
            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 执行标量查询（返回单个值，如COUNT、MAX等）
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">SQL参数数组（防止SQL注入）</param>
        /// <returns>查询结果的第一行第一列的值，如果没有结果返回null</returns>
        public async Task<object?> ExecuteScalarAsync(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlCommand command = new SqlCommand(sql, connection);
            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteScalarAsync();
        }
    }
}
