using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IHttpClientFactory httpClientFactory, ILogger<ProductsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductList()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                var resp = await client.GetAsync("https://www.pqstec.com/InvoiceApps/values/GetProductListAll");
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("External product API returned {Status}", resp.StatusCode);
                    return StatusCode((int)resp.StatusCode, new { message = "Failed to fetch products from external API" });
                }

                var contentType = resp.Content.Headers.ContentType?.MediaType;
                var raw = await resp.Content.ReadAsStringAsync();

                // Try to parse as JSON array first
                try
                {
                    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                    var arr = JsonSerializer.Deserialize<object[]>(raw, options);
                    if (arr != null)
                    {
                        return Ok(arr);
                    }
                }
                catch { /* ignore and try NDJSON parsing below */ }

                // If raw is NDJSON (one JSON object per line) or concatenated JSON objects separated by newlines,
                // split by newlines and parse each non-empty line as JSON object.
                var results = new List<object>();
                var options2 = new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

                // Normalize line endings and split
                var lines = raw.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;
                    try
                    {
                        var obj = JsonSerializer.Deserialize<object>(trimmed, options2);
                        if (obj != null) results.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse product line: {Line}", trimmed.Length > 200 ? trimmed[..200] + "..." : trimmed);
                    }
                }

                // If we parsed any objects, return them
                if (results.Count > 0)
                {
                    return Ok(results);
                }

                // Fallback: return raw string
                return Ok(new { raw = raw });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching external products");
                return StatusCode(500, new { message = "Error fetching external products", detail = ex.Message });
            }
        }
    }
}