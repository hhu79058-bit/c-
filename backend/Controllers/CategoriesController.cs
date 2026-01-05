using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 商品分类查询控制器
    /// 允许所有用户（包括未登录）查看商品分类列表
    /// </summary>
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        /// <summary>
        /// 构造函数：注入分类服务
        /// </summary>
        /// <param name="categoryService">分类服务</param>
        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// 获取所有商品分类
        /// GET /api/categories
        /// </summary>
        /// <returns>分类列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }
    }
}
