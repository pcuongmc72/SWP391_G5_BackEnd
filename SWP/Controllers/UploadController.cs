using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SWP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Không tìm thấy file tải lên." });
            }

            try
            {
                // Thư mục lưu trữ: wwwroot/uploads
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
                
                // Đảm bảo thư mục tồn tại
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Đổi tên file để tránh trùng lặp
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file vào ổ cứng
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Trả về URL của file (đường dẫn tương đối)
                var fileUrl = $"/uploads/{uniqueFileName}";

                return Ok(new
                {
                    data = new {
                        url = fileUrl,
                        fileName = file.FileName,
                        size = file.Length
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lưu file: " + ex.Message });
            }
        }
    }
}
