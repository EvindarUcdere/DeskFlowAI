namespace DeskFlowAI.Models;

public sealed class Customer
{
    private Customer()
    {
        CompanyName = string.Empty;
        ContactName = string.Empty;
        Email = string.Empty;
        Status = string.Empty;
    }

    public Customer(string companyName, string contactName, string email, string status)
    {
        CompanyName = companyName;
        ContactName = contactName;
        Email = email;
        Status = status;
    }

    public Customer(int id, string companyName, string contactName, string email, string status)
    {
        Id = id;
        CompanyName = companyName;
        ContactName = contactName;
        Email = email;
        Status = status;
    }

    public int Id { get; private set; }

    public string CompanyName { get; private set; }

    public string ContactName { get; private set; }

    public string Email { get; private set; }

    public string Status { get; private set; }

    public List<WorkProject> Projects { get; private set; } = [];

    public void UpdateDetails(string companyName, string contactName, string email)
    {
        CompanyName = companyName;
        ContactName = contactName;
        Email = email;
    }
}
