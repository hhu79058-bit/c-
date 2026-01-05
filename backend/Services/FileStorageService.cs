namespace WaiMai.Backend.Services
{
    /// <summary>
    /// 文件存储服务
    /// 负责处理文件上传、保存到本地磁盘
    /// </summary>
    public class FileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// 保存上传的图片文件
        /// </summary>
        /// <param name="file">上传的文件对象</param>
        /// <returns>文件的相对访问路径（如 /uploads/images/xxx.jpg）</returns>
        public async Task<string> SaveImageAsync(IFormFile file)
        {
            // 1. 验证文件
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("文件不能为空");
            }

            // 简单验证文件类型（仅限图片）
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("不支持的文件类型，仅支持 jpg, png, gif, webp");
            }

            // 2. 准备存储路径
            // 存储在 wwwroot/uploads/images 目录下
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 3. 生成唯一文件名（防止重名覆盖）
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 4. 保存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. 返回相对URL路径（注意：Windows路径分隔符需转换为Web标准斜杠）
            return $"/uploads/images/{uniqueFileName}";
        }
    }
}
