Employee Admin Portal - README

Overview

This repository contains two projects:
- `EmployeeAdminPortal` - ASP.NET Core Web API (Identity, EF Core, Dapper, file exports)
- `EmployeeAdminClient` - MVC client app (Razor views, jQuery)

Both target .NET 8.

Quick start (development)

1. Prerequisites
   - .NET 8 SDK
   - SQL Server LocalDB (optional) or use built-in SQLite fallback
   - (Optional) Postman for testing

2. Configure
   - API: `EmployeeAdminPortal/appsettings.json` contains `ConnectionStrings:DefaultConnection` and `Jwt` settings.
     - By default the project uses LocalDB if `DefaultConnection` is set, otherwise it uses a dev SQLite file named `employeeadmin-dev.db`.
   - Client: `EmployeeAdminClient/appsettings.json` contains `ApiSettings:BaseUrl` (default `https://localhost:7115`).

3. Run migrations and start API
   - Open a terminal at `EmployeeAdminPortal/EmployeeAdminPortal`
   - Apply migrations (optional when using LocalDB):
     dotnet ef database update
   - Run API:
     dotnet run --urls https://localhost:7115

4. Start Client
   - Open a terminal at `EmployeeAdminPortal/EmployeeAdminClient`
   - Run client:
     dotnet run --urls https://localhost:7277

API Endpoints (summary)

Authentication
- POST /api/Auth/register
  - Body: { "username":"user@test.com", "email":"user@test.com", "password":"P@ssword123!" }
- POST /api/Auth/login
  - Body: { "username":"user@test.com", "password":"P@ssword123!" }
  - Response: { "token":"<JWT>" }

Employees (protected)
- GET /api/employees
- GET /api/employees/{id}
- POST /api/employees
  - Body: {
      "Name":"Name",
      "Email":"email@test.com",
      "Phone":"0123456",
      "Salary":123.45,
      "DepartmentId":"<guid>",
      "DesignationId":"<guid>"
    }
- PUT /api/employees/{id}
- DELETE /api/employees/{id}

Lookups
- GET /api/lookup/departments
- GET /api/lookup/designations

Products (external integration)
- GET /api/products
  - Proxies https://www.pqstec.com/InvoiceApps/values/GetProductListAll and returns a JSON array (handles NDJSON-like responses).

Files & Exports
- POST /api/files/upload (Admin only) - form-data file upload
- GET /api/files/list (Admin only)
- GET /api/files/download/{fileName} (Admin only)
- GET /api/files/export/employees/csv (Authenticated)
- GET /api/files/export/employees/xlsx (Authenticated)

Client
- Client views are available on the client app (default https://localhost:7277)
  - /Auth/Login
  - /Employee (list)
  - /Employee/AddEdit (create/edit)
  - /Products (external product list)
  - /Files/Upload and /Files/Manage (uploads and exports)

Seeded data & roles
- On startup the API seeds lookup data (Departments, Designations) and roles (`Admin`, `User`).
- To create a test admin user manually:
  - Register a user via POST /api/Auth/register
  - Promote the user to Admin using SQL or in code by adding the role to the user (instructions below).

Create Admin user (SQL example for LocalDB)
- If using LocalDB open SQL Server Object Explorer and run:
  -- find the user's Id from AspNetUsers then run:
  INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('<userId>','<adminRoleId>');

Troubleshooting
- 500 errors during login/registration are often due to missing DB tables. Run `dotnet ef database update` or delete `employeeadmin-dev.db` to allow the app to create the DB on startup.
- JWT key must be sufficiently long (HS256 >= 128 bits). Edit `appsettings.json` to set `Jwt:Key`.
- If products don't render correctly, check `/api/products` response (the proxy normalizes NDJSON to an array).

Example curl commands

Register:
curl -X POST "https://localhost:7115/api/Auth/register" -H "Content-Type: application/json" -d '{"username":"testuser@test.com","email":"testuser@test.com","password":"P@ssword123!"}'

Login:
curl -X POST "https://localhost:7115/api/Auth/login" -H "Content-Type: application/json" -d '{"username":"testuser@test.com","password":"P@ssword123!"}'

Create employee (replace <token> and GUIDs):
curl -X POST "https://localhost:7115/api/employees" -H "Authorization: Bearer <token>" -H "Content-Type: application/json" -d '{"Name":"dhrubo","Email":"dhrubo@test.com","Phone":"012345678","Salary":95000,"DepartmentId":"<dept-guid>","DesignationId":"<desig-guid>"}'

Postman collection
- See `docs/postman_collection.json` in the repo (contains basic requests)

Next steps I can complete one-by-one
1. Seed Admin user automatically on startup (I can add code to `Program.cs`).
2. Add PDF export generation (QuestPDF) and client UI.
3. Add README screenshots and a short demo script.

Tell me which of the above to implement next and I will proceed.