using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoEmployeeService
{
    private readonly DeskFlowDbContext _dbContext = new();

    public List<Employee> GetEmployees()
    {
        return _dbContext.Employees
            .AsNoTracking()
            .Include(employee => employee.AssignedTasks)
            .OrderBy(employee => employee.Department)
            .ThenByDescending(employee => employee.AvailabilityStatus == EmployeeAvailabilityNames.OnLeave)
            .ThenByDescending(employee => employee.AvailabilityStatus == EmployeeAvailabilityNames.EmergencyCover)
            .ThenBy(employee => employee.FullName)
            .ToList();
    }

    public Employee CreateEmployee(
        string fullName,
        string email,
        string department,
        string roleTitle,
        string availabilityStatus,
        DateTime? leaveStart,
        DateTime? leaveEnd,
        string skills,
        string backupEmployeeName)
    {
        Employee employee = new(
            fullName,
            email,
            department,
            roleTitle,
            availabilityStatus,
            leaveStart,
            leaveEnd,
            skills,
            backupEmployeeName);

        _dbContext.Employees.Add(employee);
        _dbContext.SaveChanges();

        return employee;
    }

    public Employee UpdateEmployee(
        Employee existingEmployee,
        string fullName,
        string email,
        string department,
        string roleTitle,
        string availabilityStatus,
        DateTime? leaveStart,
        DateTime? leaveEnd,
        string skills,
        string backupEmployeeName)
    {
        Employee employee = _dbContext.Employees.Single(employee => employee.Id == existingEmployee.Id);
        employee.UpdateDetails(
            fullName,
            email,
            department,
            roleTitle,
            availabilityStatus,
            leaveStart,
            leaveEnd,
            skills,
            backupEmployeeName);
        _dbContext.SaveChanges();

        return employee;
    }
}
