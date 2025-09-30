// File: EmployeeAdminClient/Controllers/ProductController.cs

using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminClient.Controllers
{
    public class ProductController : Controller
    {
        // The URL for the required external product API
        private const string EXTERNAL_API_URL = "https://www.pqstec.com/InvoiceApps/values/GetProductListAll";

        // Action to display the product list page
        [HttpGet]
        public IActionResult Index()
        {
            // We pass the external API URL to the view so jQuery can fetch the data
            ViewBag.ExternalApiUrl = EXTERNAL_API_URL;

            // This page does not require login, but you could add a token check if required.
            return View();
        }
    }
}