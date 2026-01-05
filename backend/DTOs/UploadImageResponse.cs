namespace WaiMai.Backend.DTOs
{
    /// <summary>
    /// 图片上传响应DTO
    /// </summary>
    public class UploadImageResponse
    {
        /// <summary>
        /// 图片访问URL（相对路径，如 /uploads/xxx.jpg）
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
