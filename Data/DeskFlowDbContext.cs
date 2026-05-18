using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DeskFlowAI.Data;

public sealed class DeskFlowDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<WorkProject> Projects => Set<WorkProject>();

    public DbSet<WorkTask> Tasks => Set<WorkTask>();

    public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    public DbSet<ProjectNote> ProjectNotes => Set<ProjectNote>();

    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        string connectionString = configuration.GetConnectionString("DeskFlowDb")
            ?? throw new InvalidOperationException("Connection string 'DeskFlowDb' was not found.");

        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(customer => customer.CompanyName).HasMaxLength(160);
            entity.Property(customer => customer.ContactName).HasMaxLength(120);
            entity.Property(customer => customer.Email).HasMaxLength(180);
            entity.Property(customer => customer.Status).HasMaxLength(40);

            entity.HasIndex(customer => customer.Email).IsUnique();
            entity.HasIndex(customer => customer.Status);
        });

        modelBuilder.Entity<WorkProject>(entity =>
        {
            entity.Property(project => project.Name).HasMaxLength(180);
            entity.Property(project => project.Status).HasMaxLength(40);

            entity.HasIndex(project => project.Status);
            entity.HasIndex(project => project.DueDate);

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
            entity.Property(task => task.BlockedBy).HasMaxLength(220);

            entity.HasIndex(task => task.Status);
            entity.HasIndex(task => task.Priority);
            entity.HasIndex(task => task.BlockedBy);
            entity.HasIndex(task => task.DueDate);

            entity.HasOne(task => task.Project)
                .WithMany(project => project.Tasks)
                .HasForeignKey(task => task.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(task => task.AssignedEmployee)
                .WithMany(employee => employee.AssignedTasks)
                .HasForeignKey(task => task.AssignedEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProjectDocument>(entity =>
        {
            entity.Property(document => document.FileName).HasMaxLength(220);
            entity.Property(document => document.FilePath).HasMaxLength(600);
            entity.Property(document => document.Status).HasMaxLength(40);
            entity.Property(document => document.UploadedByEmail).HasMaxLength(180);
            entity.Property(document => document.Notes).HasMaxLength(800);
            entity.Property(document => document.AIAnalysisStatus).HasMaxLength(40);
            entity.Property(document => document.AISummary).HasMaxLength(1200);
            entity.Property(document => document.AIRiskNotes).HasMaxLength(1200);
            entity.Property(document => document.AIProviderName).HasMaxLength(80);
            entity.Property(document => document.AIReviewStatus).HasMaxLength(40);
            entity.Property(document => document.AIReviewedByEmail).HasMaxLength(180);
            entity.Property(document => document.AIRiskLevel).HasMaxLength(20);
            entity.Property(document => document.AIRecommendations).HasMaxLength(1200);
            entity.Property(document => document.AIDetectedIssues).HasMaxLength(1200);
            entity.Property(document => document.AIComplianceStatus).HasMaxLength(60);
            entity.Property(document => document.AIPolicyViolations).HasMaxLength(1200);
            entity.Property(document => document.AIProcessingPolicy).HasMaxLength(60);
            entity.Property(document => document.FileCheckStatus).HasMaxLength(40);
            entity.Property(document => document.FileCheckMessage).HasMaxLength(800);
            entity.Property(document => document.TextExtractionStatus).HasMaxLength(40);
            entity.Property(document => document.ExtractedTextPreview).HasMaxLength(1600);

            entity.HasIndex(document => document.ProjectId);
            entity.HasIndex(document => document.Status);
            entity.HasIndex(document => document.AIAnalysisStatus);
            entity.HasIndex(document => document.AIProviderName);
            entity.HasIndex(document => document.AIReviewStatus);
            entity.HasIndex(document => document.AIRiskLevel);
            entity.HasIndex(document => document.AIRiskScore);
            entity.HasIndex(document => document.AIComplianceStatus);
            entity.HasIndex(document => document.AIProcessingPolicy);
            entity.HasIndex(document => document.FileCheckStatus);
            entity.HasIndex(document => document.TextExtractionStatus);
            entity.HasIndex(document => document.UploadedAt);

            entity.HasOne(document => document.Project)
                .WithMany(project => project.Documents)
                .HasForeignKey(document => document.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
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

            entity.HasIndex(employee => employee.Email).IsUnique();
            entity.HasIndex(employee => employee.AvailabilityStatus);
            entity.HasIndex(employee => employee.Department);
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.Property(user => user.Email).HasMaxLength(180);
            entity.Property(user => user.PasswordHash).HasMaxLength(128);
            entity.Property(user => user.Role).HasMaxLength(40);

            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasIndex(user => user.Role);
            entity.HasIndex(user => user.EmployeeId)
                .IsUnique()
                .HasFilter("[EmployeeId] IS NOT NULL");

            entity.HasOne(user => user.Employee)
                .WithMany()
                .HasForeignKey(user => user.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.Property(auditLog => auditLog.ActorEmail).HasMaxLength(180);
            entity.Property(auditLog => auditLog.Action).HasMaxLength(80);
            entity.Property(auditLog => auditLog.EntityName).HasMaxLength(80);
            entity.Property(auditLog => auditLog.Details).HasMaxLength(800);

            entity.HasIndex(auditLog => auditLog.OccurredAt);
            entity.HasIndex(auditLog => auditLog.EntityName);
        });

        modelBuilder.Entity<ProjectNote>(entity =>
        {
            entity.Property(note => note.Message).HasMaxLength(1000);
            entity.Property(note => note.CreatedByEmail).HasMaxLength(180);

            entity.HasIndex(note => note.ProjectId);
            entity.HasIndex(note => note.CreatedAt);

            entity.HasOne(note => note.Project)
                .WithMany()
                .HasForeignKey(note => note.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.Property(notification => notification.RecipientEmail).HasMaxLength(180);
            entity.Property(notification => notification.Title).HasMaxLength(120);
            entity.Property(notification => notification.Message).HasMaxLength(1000);
            entity.Property(notification => notification.Severity).HasMaxLength(40);
            entity.Property(notification => notification.CreatedByEmail).HasMaxLength(180);

            entity.HasIndex(notification => notification.RecipientEmail);
            entity.HasIndex(notification => notification.IsRead);
            entity.HasIndex(notification => notification.CreatedAt);
            entity.HasIndex(notification => notification.RelatedProjectId);
        });
    }
}
