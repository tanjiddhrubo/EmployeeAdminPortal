using System.Net.Http.Json;

namespace EmployeeAdminPortal.Repositories.Implementations
{
    public class ProductRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductRepository(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<object>> GetProductsAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var resp = await client.GetAsync("https://www.pqstec.com/InvoiceApps/values/GetProductListAll");
            resp.EnsureSuccessStatusCode();
            var list = await resp.Content.ReadFromJsonAsync<IEnumerable<object>>();
            return list ?? Enumerable.Empty<object>();
        }
    }
}