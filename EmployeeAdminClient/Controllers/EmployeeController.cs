using EmployeeAdminClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EmployeeAdminClient.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public EmployeeController(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl")
                          ?? throw new InvalidOperationException("ApiSettings:BaseUrl not configured.");
        }

        // GET: Employee/Index
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/api/employees");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = $"Failed to load employees. API Status: {response.StatusCode}";
                ViewBag.ApiBaseUrl = _apiBaseUrl;
                return View(new List<EmployeeViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var employees = JsonSerializer.Deserialize<List<EmployeeViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            ViewBag.ApiBaseUrl = _apiBaseUrl;
            return View(employees);
        }

        // GET: Employee/AddEdit/{id?}
        [HttpGet]
        public IActionResult AddEdit(Guid? id)
        {
            // Ensure the user is authenticated (session contains token)
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Pass API base URL and optional employee id to the view via ViewBag
            ViewBag.ApiBaseUrl = _apiBaseUrl;
            ViewBag.EmployeeId = id;

            return View();
        }

        // Add/Edit/Delete actions would go here, similar pattern
    }
}