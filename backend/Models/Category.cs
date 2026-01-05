namespace WaiMai.Backend.Models
{
    /// <summary>
    /// 商品分类实体类
    /// 用于对菜品进行分类管理（如：热菜、凉菜、主食、饮料等）
    /// </summary>
    public class Category
    {
        /// <summary>
        /// 分类ID（主键，自增）
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
    }
}
