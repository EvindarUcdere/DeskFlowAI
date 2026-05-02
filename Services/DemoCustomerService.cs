using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DemoCustomerService
{
    private int _nextId = 4;

    public List<Customer> GetCustomers()
    {
        return
        [
            new Customer(1, "Northwind Consulting", "Aylin Kara", "aylin@northwind.example", "Active"),
            new Customer(2, "BluePeak Logistics", "Mert Yilmaz", "mert@bluepeak.example", "Active"),
            new Customer(3, "Atlas Finance", "Selin Demir", "selin@atlas.example", "On Hold")
        ];
    }

    public Customer CreateCustomer(string companyName, string contactName, string email)
    {
        Customer customer = new(_nextId, companyName, contactName, email, "Active");
        _nextId++;

        return customer;
    }
}
