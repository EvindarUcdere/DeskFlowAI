using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoCustomerService
{
    private readonly DeskFlowDbContext _dbContext = new();

    public List<Customer> GetCustomers()
    {
        return _dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.CompanyName)
            .ToList();
    }

    public Customer CreateCustomer(string companyName, string contactName, string email)
    {
        Customer customer = new(companyName, contactName, email, "Active");

        _dbContext.Customers.Add(customer);
        _dbContext.SaveChanges();

        return customer;
    }

    public Customer UpdateCustomer(Customer existingCustomer, string companyName, string contactName, string email)
    {
        Customer customer = _dbContext.Customers.Single(customer => customer.Id == existingCustomer.Id);
        customer.UpdateDetails(companyName, contactName, email);
        _dbContext.SaveChanges();

        return customer;
    }

    public void DeleteCustomer(Customer customer)
    {
        Customer customerToDelete = _dbContext.Customers.Single(existingCustomer => existingCustomer.Id == customer.Id);
        _dbContext.Customers.Remove(customerToDelete);
        _dbContext.SaveChanges();
    }
}
