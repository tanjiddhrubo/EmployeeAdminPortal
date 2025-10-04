-- Stored procedures for reads

IF OBJECT_ID('Proc_GetAllEmployees', 'P') IS NOT NULL
    DROP PROCEDURE Proc_GetAllEmployees;
GO
CREATE PROCEDURE Proc_GetAllEmployees
AS
BEGIN
    SET NOCOUNT ON;
    SELECT E.Id, E.Name, E.Email, E.Phone, E.Salary, E.DepartmentId, E.DesignationId, D.Name AS DepartmentName, G.Name AS DesignationName
    FROM Employees E
    INNER JOIN Departments D ON E.DepartmentId = D.Id
    INNER JOIN Designations G ON E.DesignationId = G.Id;
END
GO

IF OBJECT_ID('Proc_GetEmployeeById', 'P') IS NOT NULL
    DROP PROCEDURE Proc_GetEmployeeById;
GO
CREATE PROCEDURE Proc_GetEmployeeById
    @EmployeeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT E.Id, E.Name, E.Email, E.Phone, E.Salary, E.DepartmentId, E.DesignationId, D.Id AS DeptId, D.Name AS DepartmentName, G.Id AS DesigId, G.Name AS DesignationName
    FROM Employees E
    INNER JOIN Departments D ON E.DepartmentId = D.Id
    INNER JOIN Designations G ON E.DesignationId = G.Id
    WHERE E.Id = @EmployeeId;
END
GO
