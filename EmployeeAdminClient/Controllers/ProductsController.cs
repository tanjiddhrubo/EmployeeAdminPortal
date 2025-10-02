using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminClient.Controllers
{
    public class ProductsController : Controller
    {
        private readonly string _apiBaseUrl;

        public ProductsController(IConfiguration configuration)
        {
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "https://localhost:7115";
        }

        public IActionResult Index()
        {
            ViewBag.ApiBaseUrl = _apiBaseUrl;
            return View();
        }
    }
}