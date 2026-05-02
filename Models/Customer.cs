namespace DeskFlowAI.Models;

public sealed class Customer
{
    public Customer(int id, string companyName, string contactName, string email, string status)
    {
        Id = id;
        CompanyName = companyName;
        ContactName = contactName;
        Email = email;
        Status = status;
    }

    public int Id { get; }

    public string CompanyName { get; }

    public string ContactName { get; }

    public string Email { get; }

    public string Status { get; }
}
