// File: EmployeeAdminClient/Models/ProductViewModel.cs

namespace EmployeeAdminClient.Models
{
    // This model reflects the data structure returned by the external API 
    // (https://www.pqstec.com/InvoiceApps/values/GetProductListAll)
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public decimal SalesPrice { get; set; }
        public string UnitName { get; set; } = string.Empty;
    }
}