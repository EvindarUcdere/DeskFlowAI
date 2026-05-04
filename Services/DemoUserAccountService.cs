using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoUserAccountService
{
    private readonly DeskFlowDbContext _dbContext = new();

    public List<UserAccount> GetUserAccounts()
    {
        return _dbContext.UserAccounts
            .AsNoTracking()
            .Include(user => user.Employee)
            .OrderBy(user => user.Role)
            .ThenBy(user => user.Email)
            .ToList();
    }

    public UserAccount CreateUser(string email, string password, string role, int? employeeId, bool isActive)
    {
        UserAccount user = new(
            email,
            DemoPasswordHasher.Hash(password),
            role,
            employeeId,
            isActive);

        _dbContext.UserAccounts.Add(user);
        _dbContext.SaveChanges();

        return user;
    }

    public UserAccount UpdateUser(UserAccount existingUser, string email, string role, int? employeeId, bool isActive)
    {
        UserAccount user = _dbContext.UserAccounts.Single(user => user.Id == existingUser.Id);
        user.UpdateDetails(email, role, employeeId, isActive);
        _dbContext.SaveChanges();

        return user;
    }

    public UserAccount ResetPassword(UserAccount existingUser, string password)
    {
        UserAccount user = _dbContext.UserAccounts.Single(user => user.Id == existingUser.Id);
        user.ResetPassword(DemoPasswordHasher.Hash(password));
        _dbContext.SaveChanges();

        return user;
    }
}
