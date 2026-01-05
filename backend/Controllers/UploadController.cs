using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaiMai.Backend.DTOs;
using WaiMai.Backend.Services;

namespace WaiMai.Backend.Controllers
{
    /// <summary>
    /// 文件上传控制器
    /// 处理图片等文件的上传请求
    /// </summary>
    [ApiController]
    [Route("api/upload")]
    [Authorize] // 需要登录才能上传
    public class UploadController : ControllerBase
    {
        private readonly FileStorageService _fileStorageService;

        public UploadController(FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// 上传图片接口
        /// POST /api/upload/image
        /// Content-Type: multipart/form-data
        /// </summary>
        /// <param name="file">表单字段名为 file 的文件</param>
        /// <returns>图片URL</returns>
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                string url = await _fileStorageService.SaveImageAsync(file);
                return Ok(new UploadImageResponse { Url = url });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // 生产环境建议记录日志，不直接返回详细错误
                return StatusCode(500, new { message = "文件上传失败", error = ex.Message });
            }
        }
    }
}
