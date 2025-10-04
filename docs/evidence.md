Evidence and SQL outputs

1) Stored procedure output sample
- After applying `sql/ddl.sql` and `sql/dml_sample.sql`, run:
  EXEC Proc_GetAllEmployees;
- Expected: returns rows with Id, Name, Email, Phone, Salary, DepartmentName, DesignationName.

2) Trigger behavior
- A trigger `trg_Employee_Insert` is created at startup for SQL Server and inserts into `EmployeeAudit` on inserts.
- Verify by inserting a new employee and running:
  SELECT * FROM EmployeeAudit;

3) Function `ufn_GetEmployeeSummary`
- Returns per-department employee count and total salary:
  SELECT * FROM dbo.ufn_GetEmployeeSummary();

4) Exports
- CSV, XLSX and PDF export endpoints are available:
  - /api/files/export/employees/csv
  - /api/files/export/employees/xlsx
  - /api/pdf/employees
- Example: open /Files/Manage in the client and use Download buttons to save files.

5) Product integration
- /api/products proxies external NDJSON and returns JSON array.

6) Files uploaded stored in `Uploads/` folder in the API project root.

7) Sample Admin credentials
- admin@test.com / P@ssword123!


Include these artifacts with your submission. If you want I can prepare a ZIP containing the exported CSV/XLSX/PDF for the seeded data.