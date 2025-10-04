-- DDL for EmployeeAdminPortal (for reviewers)
-- Use on SQL Server

CREATE TABLE Departments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL
);

CREATE TABLE Designations (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL
);

CREATE TABLE Employees (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(400) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Salary DECIMAL(18,2) NOT NULL,
    DepartmentId UNIQUEIDENTIFIER NOT NULL,
    DesignationId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_Employees_Designations FOREIGN KEY (DesignationId) REFERENCES Designations(Id)
);

-- Identity tables are created by ASP.NET Identity via EF migrations; include reference scripts in Migrations folder.
