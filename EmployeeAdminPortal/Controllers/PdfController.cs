using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EmployeeAdminPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PdfController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("employees")]
        public async Task<IActionResult> ExportEmployeesPdf()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .ToListAsync();

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Element(header =>
                    {
                        header.AlignCenter();
                        header.PaddingBottom(10);
                        header.Text("Employee Report").SemiBold().FontSize(18);
                    });

                    page.Content().Element(content =>
                    {
                        content.Table(table =>
                        {
                            // columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1); // Id
                                columns.RelativeColumn(3); // Name
                                columns.RelativeColumn(3); // Email
                                columns.RelativeColumn(2); // Phone
                                columns.RelativeColumn(2); // Salary
                                columns.RelativeColumn(2); // Dept
                                columns.RelativeColumn(2); // Desig
                            });

                            // header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Id");
                                header.Cell().Element(CellStyle).Text("Name");
                                header.Cell().Element(CellStyle).Text("Email");
                                header.Cell().Element(CellStyle).Text("Phone");
                                header.Cell().Element(CellStyle).Text("Salary");
                                header.Cell().Element(CellStyle).Text("Department");
                                header.Cell().Element(CellStyle).Text("Designation");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).Padding(5).Background(Colors.Grey.Lighten3);
                                }
                            });

                            // rows
                            foreach (var e in employees)
                            {
                                table.Cell().Element(CellContent).Text(e.Id.ToString());
                                table.Cell().Element(CellContent).Text(e.Name);
                                table.Cell().Element(CellContent).Text(e.Email);
                                table.Cell().Element(CellContent).Text(e.Phone ?? string.Empty);
                                table.Cell().Element(CellContent).Text(e.Salary.ToString("F2"));
                                table.Cell().Element(CellContent).Text(e.Department?.Name ?? string.Empty);
                                table.Cell().Element(CellContent).Text(e.Designation?.Name ?? string.Empty);

                                static IContainer CellContent(IContainer container)
                                {
                                    return container.Padding(5);
                                }
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated: ");
                        x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")).SemiBold();
                    });
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            ms.Position = 0;
            return File(ms.ToArray(), "application/pdf", "employees.pdf");
        }
    }
}
