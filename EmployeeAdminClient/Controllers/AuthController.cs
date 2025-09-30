using EmployeeAdminClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace EmployeeAdminClient.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        // Constructor uses IConfiguration to read the API Base URL from appsettings.json
        public AuthController(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl")
                          ?? throw new InvalidOperationException("ApiSettings:BaseUrl not configured.");
        }

        // -------------------------------------------------------------------
        // LOGIN ACTIONS
        // -------------------------------------------------------------------

        [HttpGet]
        public IActionResult Login()
        {
            // Clear any existing token on the way to the login page
            HttpContext.Session.Remove("JwtToken");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // List of potential login endpoints to try
            var endpointsToTry = new List<string>
            {
                "/api/auth/login",
                "/api/Account/Login"
            };

            // NEW: Define the different key names the API might accept for the identifier
            var identifierKeysToTry = new List<string> { "username", "email" };

            string? jwtToken = null;
            HttpResponseMessage? finalResponse = null;

            // Loop through identifier keys (username then email)
            foreach (var key in identifierKeysToTry)
            {
                // Prepare the JSON payload dynamically based on the current key
                var loginRequestData = new Dictionary<string, string>
                {
                    { key, model.Username },
                    { "password", model.Password }
                };
                var loginRequestJson = JsonSerializer.Serialize(loginRequestData);

                // Loop through API endpoints
                foreach (var endpoint in endpointsToTry)
                {
                    try
                    {
                        var content = new StringContent(loginRequestJson, Encoding.UTF8, "application/json");
                        var response = await _httpClient.PostAsync($"{_apiBaseUrl}{endpoint}", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            var tokenData = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);

                            if (tokenData != null && tokenData.TryGetValue("token", out jwtToken))
                            {
                                // Success! Store the response and use goto to break all nested loops.
                                finalResponse = response;
                                goto SuccessFlow;
                            }
                        }

                        // Capture the response for error reporting if all attempts fail
                        finalResponse = response;

                        // If we failed but not with a 400 Bad Request, break the inner endpoint loop 
                        // and try the next key (username/email).
                        if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, $"Critical error trying API login: {ex.Message}");
                        return View(model);
                    }
                }
            }

        SuccessFlow: // Label for success jump

            // If we have a token, authentication was successful
            if (!string.IsNullOrEmpty(jwtToken))
            {
                HttpContext.Session.SetString("JwtToken", jwtToken);
                return RedirectToAction("Index", "Employee");
            }

            // If we reach here, login failed with all attempts. Report the error.
            if (finalResponse != null)
            {
                var responseContent = await finalResponse.Content.ReadAsStringAsync();
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
                    if (errorResponse != null && errorResponse.TryGetValue("message", out var errorMessage))
                    {
                        ModelState.AddModelError(string.Empty, errorMessage);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"Login failed: {finalResponse.StatusCode}. Check credentials/API configuration.");
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt or unhandled API error. (Check credentials and API URL)");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Failed to connect to the API base URL.");
            }

            return View(model);
        }

        // -------------------------------------------------------------------
        // LOGOUT ACTION
        // -------------------------------------------------------------------

        [HttpGet]
        public IActionResult Logout()
        {
            // Clear the session token and redirect back to login
            HttpContext.Session.Remove("JwtToken");
            return RedirectToAction("Login");
        }

        // -------------------------------------------------------------------
        // REGISTER ACTIONS
        // -------------------------------------------------------------------

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // IMPORTANT: Set the default role to prevent client-side tampering
            model.Roles = new List<string> { "User" };

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Prepare the JSON payload for the API
                var registerRequest = new
                {
                    username = model.Username,
                    email = model.Email,
                    password = model.Password,
                    roles = model.Roles // Hardcoded to ["User"]
                };

                var registerRequestJson = JsonSerializer.Serialize(registerRequest);
                var content = new StringContent(registerRequestJson, Encoding.UTF8, "application/json");

                // Use the correct API endpoint: /api/Auth/register
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Auth/register", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Store a success message to display on the login page
                    TempData["SuccessMessage"] = $"Registration successful for {model.Email}. Please log in now.";
                    return RedirectToAction("Login");
                }

                // Handle registration failure
                ModelState.AddModelError(string.Empty, $"Registration failed. API Status: {response.StatusCode}");

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
                    if (errorResponse != null && errorResponse.TryGetValue("message", out var errorMessage))
                    {
                        ModelState.AddModelError(string.Empty, errorMessage);
                    }
                    else if (errorResponse != null && errorResponse.ContainsKey("errors"))
                    {
                        ModelState.AddModelError(string.Empty, "Registration failed due to validation issues. Check password complexity/email uniqueness.");
                    }
                }
                catch
                {
                    // Fallback for non-JSON responses
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            return View(model);
        }
    }
}
