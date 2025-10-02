using EmployeeAdminClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace EmployeeAdminClient.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly ILogger<AuthController> _logger;

        // Constructor uses IConfiguration to read the API Base URL from appsettings.json
        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;

            var configured = configuration.GetValue<string>("ApiSettings:BaseUrl");
            if (string.IsNullOrWhiteSpace(configured))
            {
                // fallback to expected default API url
                configured = "https://localhost:7115";
                _logger.LogWarning("ApiSettings:BaseUrl not configured; falling back to {Url}", configured);
            }

            _apiBaseUrl = configured.TrimEnd('/');
        }

        // -------------------------------------------------------------------
        // LOGIN ACTIONS
        // -------------------------------------------------------------------

        [HttpGet("Login")]
        public IActionResult Login()
        {
            _logger.LogDebug("GET {Path} invoked", HttpContext.Request.Path);
            // Clear any existing token on the way to the login page
            HttpContext.Session.Remove("JwtToken");
            return View();
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogDebug("POST {Path} invoked", HttpContext.Request.Path);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Use a single well-known endpoint to avoid ambiguity
            var endpoint = "/api/auth/login";

            string? jwtToken = null;
            HttpResponseMessage? response = null;

            try
            {
                var loginRequest = new Dictionary<string, string>
                {
                    { "username", model.Username },
                    { "password", model.Password }
                };

                var loginRequestJson = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(loginRequestJson, Encoding.UTF8, "application/json");

                var requestUrl = _apiBaseUrl + endpoint;
                _logger.LogInformation("Attempting login POST to {Url} for user {User}", requestUrl, model.Username);

                response = await _httpClient.PostAsync(requestUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Login response ({Status}): {Body}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var tokenData = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
                    if (tokenData != null && tokenData.TryGetValue("token", out jwtToken))
                    {
                        HttpContext.Session.SetString("JwtToken", jwtToken);
                        // Redirect to Add Employee page in the CLIENT after login
                        return RedirectToAction("AddEdit", "Employee");
                    }

                    // If no token present, treat as failure
                    ModelState.AddModelError(string.Empty, "Login succeeded but no token was returned by the API.");
                    return View(model);
                }

                // Non-success status: try to present API message
                try
                {
                    var err = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    if (err != null && err.TryGetValue("message", out var msg))
                    {
                        ModelState.AddModelError(string.Empty, msg?.ToString() ?? "Login failed.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"Login failed: {response.StatusCode}");
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, $"Login failed: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when calling login endpoint {Url}", _apiBaseUrl + endpoint);
                ModelState.AddModelError(string.Empty, "Could not reach authentication service. Check API is running and ApiSettings:BaseUrl is correct.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during login.");
            }

            return View(model);
        }

        // -------------------------------------------------------------------
        // LOGOUT ACTION
        // -------------------------------------------------------------------

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            _logger.LogDebug("GET {Path} invoked", HttpContext.Request.Path);
            // Clear the session token and redirect back to login
            HttpContext.Session.Remove("JwtToken");
            return RedirectToAction("Login");
        }

        // -------------------------------------------------------------------
        // REGISTER ACTIONS
        // -------------------------------------------------------------------

        [HttpGet("Register")]
        public IActionResult Register()
        {
            _logger.LogDebug("GET {Path} invoked", HttpContext.Request.Path);
            return View();
        }

        [HttpPost("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogDebug("POST {Path} invoked", HttpContext.Request.Path);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Update: Match the API's RegisterRequestDto format exactly
                var registerRequest = new
                {
                    username = model.Username,
                    email = model.Email,
                    password = model.Password
                };

                var registerRequestJson = JsonSerializer.Serialize(registerRequest);
                var content = new StringContent(registerRequestJson, Encoding.UTF8, "application/json");

                var requestUrl = _apiBaseUrl + "/api/Auth/register";
                _logger.LogInformation("Sending registration POST to {Url} for user {User}", requestUrl, model.Username);

                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Registration successful for {model.Email}. Please log in now.";
                    return RedirectToAction("Login");
                }

                ModelState.AddModelError(string.Empty, $"Registration failed. API Status: {response.StatusCode}");

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    if (errorResponse != null)
                    {
                        if (errorResponse.TryGetValue("errors", out var errors))
                        {
                            var errorList = JsonSerializer.Deserialize<Dictionary<string, string[]>>(errors.ToString() ?? "{}");
                            if (errorList != null)
                            {
                                foreach (var error in errorList.SelectMany(e => e.Value))
                                {
                                    ModelState.AddModelError(string.Empty, error);
                                }
                            }
                        }
                        else if (errorResponse.TryGetValue("message", out var message))
                        {
                            ModelState.AddModelError(string.Empty, message?.ToString() ?? "Unknown error");
                        }
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            return View(model);
        }
    }
}
