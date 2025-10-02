IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Proc_GetAllEmployees')
BEGIN
    EXEC('CREATE PROCEDURE Proc_GetAllEmployees AS BEGIN SELECT E.Id, E.Name, E.Email, E.Phone, E.Salary, E.DepartmentId, E.DesignationId, D.Name AS DepartmentName, G.Name AS DesignationName FROM Employees E INNER JOIN Departments D ON E.DepartmentId = D.Id INNER JOIN Designations G ON E.DesignationId = G.Id END')
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Proc_GetEmployeeById')
BEGIN
    EXEC('CREATE PROCEDURE Proc_GetEmployeeById @EmployeeId UNIQUEIDENTIFIER AS BEGIN SELECT E.Id, E.Name, E.Email, E.Phone, E.Salary, E.DepartmentId, E.DesignationId, D.Id AS DeptId, D.Name AS DepartmentName, G.Id AS DesigId, G.Name AS DesignationName FROM Employees E INNER JOIN Departments D ON E.DepartmentId = D.Id INNER JOIN Designations G ON E.DesignationId = G.Id WHERE E.Id = @EmployeeId END')
END
