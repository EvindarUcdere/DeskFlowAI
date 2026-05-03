using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Data;

public sealed class DeskFlowDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<WorkProject> Projects => Set<WorkProject>();

    public DbSet<WorkTask> Tasks => Set<WorkTask>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=deskflow.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(customer => customer.CompanyName).HasMaxLength(160);
            entity.Property(customer => customer.ContactName).HasMaxLength(120);
            entity.Property(customer => customer.Email).HasMaxLength(180);
            entity.Property(customer => customer.Status).HasMaxLength(40);
        });

        modelBuilder.Entity<WorkProject>(entity =>
        {
            entity.Property(project => project.Name).HasMaxLength(180);
            entity.Property(project => project.Status).HasMaxLength(40);

            entity.HasOne(project => project.Customer)
                .WithMany(customer => customer.Projects)
                .HasForeignKey(project => project.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkTask>(entity =>
        {
            entity.Property(task => task.Title).HasMaxLength(180);
            entity.Property(task => task.Status).HasMaxLength(40);
            entity.Property(task => task.Priority).HasMaxLength(40);

            entity.HasOne(task => task.Project)
                .WithMany(project => project.Tasks)
                .HasForeignKey(task => task.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(task => task.AssignedEmployee)
                .WithMany(employee => employee.AssignedTasks)
                .HasForeignKey(task => task.AssignedEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(employee => employee.FullName).HasMaxLength(140);
            entity.Property(employee => employee.Email).HasMaxLength(180);
            entity.Property(employee => employee.Department).HasMaxLength(80);
            entity.Property(employee => employee.RoleTitle).HasMaxLength(100);
            entity.Property(employee => employee.AvailabilityStatus).HasMaxLength(40);
            entity.Property(employee => employee.Skills).HasMaxLength(400);
            entity.Property(employee => employee.BackupEmployeeName).HasMaxLength(140);
        });

        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.Property(auditLog => auditLog.ActorEmail).HasMaxLength(180);
            entity.Property(auditLog => auditLog.Action).HasMaxLength(80);
            entity.Property(auditLog => auditLog.EntityName).HasMaxLength(80);
            entity.Property(auditLog => auditLog.Details).HasMaxLength(800);
        });
    }
}
