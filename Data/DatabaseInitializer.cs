using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Data;

public sealed class DatabaseInitializer
{
    public void Initialize()
    {
        using DeskFlowDbContext dbContext = new();
        dbContext.Database.Migrate();
        SeedCustomersIfNeeded(dbContext);
    }

    private static void SeedCustomersIfNeeded(DeskFlowDbContext dbContext)
    {
        if (dbContext.Customers.Any())
        {
            return;
        }

        dbContext.Customers.AddRange(
            new Customer("Northwind Consulting", "Aylin Kara", "aylin@northwind.example", "Active"),
            new Customer("BluePeak Logistics", "Mert Yilmaz", "mert@bluepeak.example", "Active"),
            new Customer("Atlas Finance", "Selin Demir", "selin@atlas.example", "On Hold"));

        dbContext.SaveChanges();
    }
}
