using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins should manage file uploads/downloads
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        // IWebHostEnvironment provides information about the web hosting environment (e.g., content root path)
        public FilesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // POST: /api/files/upload
        // Accepts one or more files via form-data
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // Define the path to the Uploads folder
            var uploadsPath = Path.Combine(_env.ContentRootPath, "Uploads");

            // Ensure the folder exists
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Create a unique file name to avoid overwrites
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save the file to the disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the unique file name/path so the client can request it later
            return Ok(new
            {
                FileName = fileName,
                DownloadUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Uploads/{fileName}"
            });
        }

        // GET: /api/files/download/{fileName}
        [HttpGet]
        [Route("download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be empty.");
            }

            var uploadsPath = Path.Combine(_env.ContentRootPath, "Uploads");
            var filePath = Path.Combine(uploadsPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            // Determine content type based on file extension
            var mimeType = GetMimeType(filePath);

            // Return the file as a downloadable attachment
            return File(System.IO.File.OpenRead(filePath), mimeType, fileName);
        }

        // Helper function to guess MIME type (basic implementation)
        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream", // Default for unknown types
            };
        }
    }
}