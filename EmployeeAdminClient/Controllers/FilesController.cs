using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminClient.Controllers
{
    public class FilesController : Controller
    {
        private readonly string _apiBaseUrl;

        public FilesController(IConfiguration configuration)
        {
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "https://localhost:7115";
        }

        public IActionResult Upload()
        {
            ViewBag.ApiBaseUrl = _apiBaseUrl;
            return View();
        }

        public IActionResult Manage()
        {
            ViewBag.ApiBaseUrl = _apiBaseUrl;
            return View();
        }
    }
}