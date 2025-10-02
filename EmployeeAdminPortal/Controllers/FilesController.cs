using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeAdminPortal.Data;
using ClosedXML.Excel;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IWebHostEnvironment env, ApplicationDbContext context, ILogger<FilesController> logger)
        {
            _env = env;
            _context = context;
            _logger = logger;
        }

        // POST: /api/files/upload - Admin only
        // Accepts one file via form-data
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded" });
            }

            // Define the path to the Uploads folder
            var uploadsPath = Path.Combine(_env.ContentRootPath, "Uploads");

            // Ensure the folder exists
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            // Create a unique file name to avoid overwrites
            var fileName = Path.GetFileName(file.FileName);
            var safeName = $"{Guid.NewGuid():N}_{fileName}";
            var filePath = Path.Combine(uploadsPath, safeName);

            try
            {
                // Save the file to the disk
                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                return Ok(new { fileName = safeName, original = fileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File upload failed");
                return StatusCode(500, new { message = "File upload failed", detail = ex.Message });
            }
        }

        // GET: /api/files/list - Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("list")]
        public IActionResult List()
        {
            var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsDir)) return Ok(new string[0]);

            var files = Directory.GetFiles(uploadsDir)
                .Select(Path.GetFileName)
                .OrderByDescending(n => n)
                .ToArray();

            return Ok(files);
        }

        // GET: /api/files/download/{fileName} - Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return BadRequest();
            var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads");
            var filePath = Path.Combine(uploadsDir, fileName);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var contentType = GetContentType(filePath);
            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, contentType, Path.GetFileName(filePath));
        }

        // GET: /api/files/export/employees/csv - Authenticated users
        [HttpGet("export/employees/csv")]
        [Authorize]
        public async Task<IActionResult> ExportEmployeesCsv()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Id,Name,Email,Phone,Salary,Department,Designation");
            foreach (var e in employees)
            {
                var line = string.Join(",",
                    EscapeCsv(e.Id.ToString()),
                    EscapeCsv(e.Name),
                    EscapeCsv(e.Email),
                    EscapeCsv(e.Phone ?? string.Empty),
                    e.Salary.ToString("F2"),
                    EscapeCsv(e.Department?.Name ?? string.Empty),
                    EscapeCsv(e.Designation?.Name ?? string.Empty)
                );
                sb.AppendLine(line);
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "employees.csv");
        }

        // GET: /api/files/export/employees/xlsx - Authenticated users
        [HttpGet("export/employees/xlsx")]
        [Authorize]
        public async Task<IActionResult> ExportEmployeesExcel()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Employees");
            // Header
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Name";
            ws.Cell(1, 3).Value = "Email";
            ws.Cell(1, 4).Value = "Phone";
            ws.Cell(1, 5).Value = "Salary";
            ws.Cell(1, 6).Value = "Department";
            ws.Cell(1, 7).Value = "Designation";

            var row = 2;
            foreach (var e in employees)
            {
                ws.Cell(row, 1).Value = e.Id.ToString();
                ws.Cell(row, 2).Value = e.Name;
                ws.Cell(row, 3).Value = e.Email;
                ws.Cell(row, 4).Value = e.Phone;
                ws.Cell(row, 5).Value = e.Salary;
                ws.Cell(row, 6).Value = e.Department?.Name;
                ws.Cell(row, 7).Value = e.Designation?.Name;
                row++;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "employees.xlsx");
        }

        private static string EscapeCsv(string input)
        {
            if (input == null) return string.Empty;
            if (input.Contains(',') || input.Contains('"') || input.Contains('\n'))
            {
                return '"' + input.Replace("\"", "\"\"") + '"';
            }
            return input;
        }

        private static string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".csv" => "text/csv",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream",
            };
        }
    }
}