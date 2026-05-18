using DeskFlowAI.Models;
using DeskFlowAI.Services;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DeskFlowAI.Data;

public sealed class DatabaseInitializer
{
    public void Initialize()
    {
        using DeskFlowDbContext dbContext = new();
        dbContext.Database.Migrate();
        SeedDemoDataIfNeeded(dbContext);
    }

    private static void SeedDemoDataIfNeeded(DeskFlowDbContext dbContext)
    {
        SeedCustomersIfNeeded(dbContext);
        SeedEmployeesIfNeeded(dbContext);
        SeedUserAccountsIfNeeded(dbContext);
        SeedProjectsIfNeeded(dbContext);
        SeedTasksIfNeeded(dbContext);
        SeedProjectDocumentsIfNeeded(dbContext);
    }

    private static void SeedCustomersIfNeeded(DeskFlowDbContext dbContext)
    {
        AddCustomerIfMissing(dbContext, "northwind@deskflow.demo", "Northwind Consulting", "Aylin Kara", "Active");
        AddCustomerIfMissing(dbContext, "bluepeak@deskflow.demo", "BluePeak Logistics", "Mert Yilmaz", "Active");
        AddCustomerIfMissing(dbContext, "atlas@deskflow.demo", "Atlas Finance", "Selin Demir", "On Hold");
        AddCustomerIfMissing(dbContext, "nova@deskflow.demo", "Nova Retail Group", "Deniz Arslan", "Active");
        AddCustomerIfMissing(dbContext, "orion@deskflow.demo", "Orion Health Systems", "Ece Kaya", "Active");

        dbContext.SaveChanges();
    }

    private static void SeedUserAccountsIfNeeded(DeskFlowDbContext dbContext)
    {
        Employee evin = GetEmployee(dbContext, "evin@deskflow.ai");
        Employee merve = GetEmployee(dbContext, "merve@deskflow.ai");
        Employee can = GetEmployee(dbContext, "can@deskflow.ai");

        AddUserAccountIfMissing(dbContext, "admin@deskflow.ai", "Admin123", RoleNames.Admin, evin.Id);
        AddUserAccountIfMissing(dbContext, "manager@deskflow.ai", "Admin123", RoleNames.Manager, merve.Id);
        AddUserAccountIfMissing(dbContext, "staff@deskflow.ai", "Admin123", RoleNames.Staff, can.Id);

        dbContext.SaveChanges();
    }

    private static void SeedEmployeesIfNeeded(DeskFlowDbContext dbContext)
    {
        AddEmployeeIfMissing(
            dbContext,
            "evin@deskflow.ai",
            "Evin D.",
            "Operations",
            "Operations Lead",
            EmployeeAvailabilityNames.Available,
            null,
            null,
            "Customer coordination, project planning, approvals",
            "Merve A.");

        AddEmployeeIfMissing(
            dbContext,
            "merve@deskflow.ai",
            "Merve A.",
            "Project Office",
            "Project Manager",
            EmployeeAvailabilityNames.Busy,
            null,
            null,
            "Risk reporting, timeline management, client meetings",
            "Evin D.");

        AddEmployeeIfMissing(
            dbContext,
            "can@deskflow.ai",
            "Can K.",
            "Delivery",
            "Implementation Specialist",
            EmployeeAvailabilityNames.Available,
            null,
            null,
            "WPF screens, SQL checks, deployment support",
            "Zeynep T.");

        AddEmployeeIfMissing(
            dbContext,
            "zeynep@deskflow.ai",
            "Zeynep T.",
            "Support",
            "Customer Support Specialist",
            EmployeeAvailabilityNames.EmergencyCover,
            null,
            null,
            "Customer tickets, training, urgent handover",
            "Can K.");

        AddEmployeeIfMissing(
            dbContext,
            "arda@deskflow.ai",
            "Arda B.",
            "Delivery",
            "Backend Developer",
            EmployeeAvailabilityNames.OnLeave,
            DateTime.Today.AddDays(2),
            DateTime.Today.AddDays(6),
            "EF Core, migrations, integrations",
            "Can K.");

        dbContext.SaveChanges();
    }

    private static void SeedProjectsIfNeeded(DeskFlowDbContext dbContext)
    {
        Customer northwind = GetCustomer(dbContext, "northwind@deskflow.demo");
        Customer bluePeak = GetCustomer(dbContext, "bluepeak@deskflow.demo");
        Customer atlas = GetCustomer(dbContext, "atlas@deskflow.demo");
        Customer nova = GetCustomer(dbContext, "nova@deskflow.demo");
        Customer orion = GetCustomer(dbContext, "orion@deskflow.demo");

        AddProjectIfMissing(dbContext, northwind.Id, "Customer Portal Renewal", ProjectStatusNames.Active, DateTime.Today.AddDays(3));
        AddProjectIfMissing(dbContext, northwind.Id, "Contract Automation Setup", ProjectStatusNames.Planning, DateTime.Today.AddDays(16));
        AddProjectIfMissing(dbContext, bluePeak.Id, "Warehouse Mobile App", ProjectStatusNames.Active, DateTime.Today.AddDays(6));
        AddProjectIfMissing(dbContext, bluePeak.Id, "Route Optimization Dashboard", ProjectStatusNames.OnHold, DateTime.Today.AddDays(28));
        AddProjectIfMissing(dbContext, atlas.Id, "Quarterly Risk Reporting", ProjectStatusNames.Active, DateTime.Today.AddDays(1));
        AddProjectIfMissing(dbContext, atlas.Id, "Legacy Data Migration", ProjectStatusNames.Completed, DateTime.Today.AddDays(-5));
        AddProjectIfMissing(dbContext, nova.Id, "E-Commerce Backoffice", ProjectStatusNames.Active, DateTime.Today.AddDays(9));
        AddProjectIfMissing(dbContext, orion.Id, "Patient Intake Workflow", ProjectStatusNames.Planning, null);

        dbContext.SaveChanges();
    }

    private static void SeedTasksIfNeeded(DeskFlowDbContext dbContext)
    {
        WorkProject customerPortal = GetProject(dbContext, "Customer Portal Renewal");
        WorkProject contractAutomation = GetProject(dbContext, "Contract Automation Setup");
        WorkProject warehouseApp = GetProject(dbContext, "Warehouse Mobile App");
        WorkProject routeDashboard = GetProject(dbContext, "Route Optimization Dashboard");
        WorkProject riskReporting = GetProject(dbContext, "Quarterly Risk Reporting");
        WorkProject dataMigration = GetProject(dbContext, "Legacy Data Migration");
        WorkProject backoffice = GetProject(dbContext, "E-Commerce Backoffice");
        WorkProject intakeWorkflow = GetProject(dbContext, "Patient Intake Workflow");

        RenameTaskIfExists(dbContext, "Login ekranini musteri markasina gore duzenle", "Customize login screen for client brand");
        RenameTaskIfExists(dbContext, "Kullanici kabul test listesini hazirla", "Prepare user acceptance test checklist");
        RenameTaskIfExists(dbContext, "Eski portal linklerini yonlendir", "Redirect legacy portal links");
        RenameTaskIfExists(dbContext, "Sozlesme sablon alanlarini belirle", "Define contract template fields");
        RenameTaskIfExists(dbContext, "Onay akis rollerini netlestir", "Clarify approval workflow roles");
        RenameTaskIfExists(dbContext, "Barkod tarama ekranini test et", "Test barcode scanning screen");
        RenameTaskIfExists(dbContext, "Offline senkronizasyon hatalarini incele", "Review offline sync errors");
        RenameTaskIfExists(dbContext, "Harita servis maliyetlerini karsilastir", "Compare map service costs");
        RenameTaskIfExists(dbContext, "Yonetim ozeti grafiklerini tamamla", "Finalize executive summary charts");
        RenameTaskIfExists(dbContext, "Excel export formatini dogrula", "Validate Excel export format");
        RenameTaskIfExists(dbContext, "Final veri kontrol raporunu arsivle", "Archive final data validation report");
        RenameTaskIfExists(dbContext, "Stok alarm kurallarini tanimla", "Define stock alert rules");
        RenameTaskIfExists(dbContext, "Iade sureci ekran metinlerini guncelle", "Update refund workflow screen copy");
        RenameTaskIfExists(dbContext, "Hasta kayit form alanlarini gozden gecir", "Review patient intake form fields");
        RenameTaskIfExists(dbContext, "KVKK onay adimini tasarla", "Design privacy consent approval step");

        AddTaskIfMissing(dbContext, customerPortal.Id, "Customize login screen for client brand", TaskStatusNames.InProgress, TaskPriorityNames.High, DateTime.Today.AddDays(1));
        AddTaskIfMissing(dbContext, customerPortal.Id, "Prepare user acceptance test checklist", TaskStatusNames.ToDo, TaskPriorityNames.Critical, DateTime.Today.AddDays(2));
        AddTaskIfMissing(dbContext, customerPortal.Id, "Redirect legacy portal links", TaskStatusNames.Blocked, TaskPriorityNames.Normal, DateTime.Today.AddDays(-1), "Customer DNS approval");
        AddTaskIfMissing(dbContext, contractAutomation.Id, "Define contract template fields", TaskStatusNames.ToDo, TaskPriorityNames.Normal, DateTime.Today.AddDays(10));
        AddTaskIfMissing(dbContext, contractAutomation.Id, "Clarify approval workflow roles", TaskStatusNames.ToDo, TaskPriorityNames.High, DateTime.Today.AddDays(14));
        AddTaskIfMissing(dbContext, warehouseApp.Id, "Test barcode scanning screen", TaskStatusNames.InProgress, TaskPriorityNames.Critical, DateTime.Today);
        AddTaskIfMissing(dbContext, warehouseApp.Id, "Review offline sync errors", TaskStatusNames.ToDo, TaskPriorityNames.High, DateTime.Today.AddDays(5));
        AddTaskIfMissing(dbContext, routeDashboard.Id, "Compare map service costs", TaskStatusNames.Blocked, TaskPriorityNames.Normal, null, "Vendor pricing response");
        AddTaskIfMissing(dbContext, riskReporting.Id, "Finalize executive summary charts", TaskStatusNames.InProgress, TaskPriorityNames.Critical, DateTime.Today.AddDays(1));
        AddTaskIfMissing(dbContext, riskReporting.Id, "Validate Excel export format", TaskStatusNames.ToDo, TaskPriorityNames.High, DateTime.Today.AddDays(3));
        AddTaskIfMissing(dbContext, dataMigration.Id, "Archive final data validation report", TaskStatusNames.Done, TaskPriorityNames.Low, DateTime.Today.AddDays(-6));
        AddTaskIfMissing(dbContext, backoffice.Id, "Define stock alert rules", TaskStatusNames.ToDo, TaskPriorityNames.High, DateTime.Today.AddDays(7));
        AddTaskIfMissing(dbContext, backoffice.Id, "Update refund workflow screen copy", TaskStatusNames.ToDo, TaskPriorityNames.Normal, DateTime.Today.AddDays(12));
        AddTaskIfMissing(dbContext, intakeWorkflow.Id, "Review patient intake form fields", TaskStatusNames.ToDo, TaskPriorityNames.Normal, null);
        AddTaskIfMissing(dbContext, intakeWorkflow.Id, "Design privacy consent approval step", TaskStatusNames.ToDo, TaskPriorityNames.High, DateTime.Today.AddDays(18));

        dbContext.SaveChanges();
        AssignDemoTasksIfNeeded(dbContext);
        dbContext.SaveChanges();
    }

    private static void AssignDemoTasksIfNeeded(DeskFlowDbContext dbContext)
    {
        Employee evin = GetEmployee(dbContext, "evin@deskflow.ai");
        Employee merve = GetEmployee(dbContext, "merve@deskflow.ai");
        Employee can = GetEmployee(dbContext, "can@deskflow.ai");
        Employee zeynep = GetEmployee(dbContext, "zeynep@deskflow.ai");
        Employee arda = GetEmployee(dbContext, "arda@deskflow.ai");

        AssignTaskIfUnassigned(dbContext, "Customize login screen for client brand", can.Id);
        AssignTaskIfUnassigned(dbContext, "Prepare user acceptance test checklist", merve.Id);
        AssignTaskIfUnassigned(dbContext, "Redirect legacy portal links", zeynep.Id);
        AssignTaskIfUnassigned(dbContext, "Define contract template fields", evin.Id);
        AssignTaskIfUnassigned(dbContext, "Clarify approval workflow roles", merve.Id);
        AssignTaskIfUnassigned(dbContext, "Test barcode scanning screen", can.Id);
        AssignTaskIfUnassigned(dbContext, "Review offline sync errors", arda.Id);
        AssignTaskIfUnassigned(dbContext, "Compare map service costs", zeynep.Id);
        AssignTaskIfUnassigned(dbContext, "Finalize executive summary charts", merve.Id);
        AssignTaskIfUnassigned(dbContext, "Validate Excel export format", evin.Id);
        AssignTaskIfUnassigned(dbContext, "Archive final data validation report", can.Id);
        AssignTaskIfUnassigned(dbContext, "Define stock alert rules", arda.Id);
        AssignTaskIfUnassigned(dbContext, "Update refund workflow screen copy", zeynep.Id);
        AssignTaskIfUnassigned(dbContext, "Review patient intake form fields", evin.Id);
        AssignTaskIfUnassigned(dbContext, "Design privacy consent approval step", merve.Id);
    }

    private static void SeedProjectDocumentsIfNeeded(DeskFlowDbContext dbContext)
    {
        WorkProject customerPortal = GetProject(dbContext, "Customer Portal Renewal");
        WorkProject warehouseApp = GetProject(dbContext, "Warehouse Mobile App");
        WorkProject riskReporting = GetProject(dbContext, "Quarterly Risk Reporting");
        WorkProject dataMigration = GetProject(dbContext, "Legacy Data Migration");
        WorkProject backoffice = GetProject(dbContext, "E-Commerce Backoffice");
        WorkProject intakeWorkflow = GetProject(dbContext, "Patient Intake Workflow");

        AddDocumentIfMissing(
            dbContext,
            customerPortal.Id,
            "customer-portal-client-brief.pdf",
            @"C:\DeskFlowDemo\Documents\Northwind\customer-portal-client-brief.pdf",
            DocumentStatusNames.InReview,
            "manager@deskflow.ai",
            "Musteri beklentileri ve yenilenecek portal kapsam notlari.");

        AddDocumentIfMissing(
            dbContext,
            customerPortal.Id,
            "portal-acceptance-checklist.xlsx",
            @"C:\DeskFlowDemo\Documents\Northwind\portal-acceptance-checklist.xlsx",
            DocumentStatusNames.Uploaded,
            "admin@deskflow.ai",
            "UAT maddeleri henuz musteri ile son kez netlestirilecek.");

        AddDocumentIfMissing(
            dbContext,
            warehouseApp.Id,
            "warehouse-mobile-test-report.docx",
            @"C:\DeskFlowDemo\Documents\BluePeak\warehouse-mobile-test-report.docx",
            DocumentStatusNames.NeedsUpdate,
            "staff@deskflow.ai",
            "Offline senkronizasyon bolumu yeniden test edilmeli.");

        AddDocumentIfMissing(
            dbContext,
            riskReporting.Id,
            "quarterly-risk-dashboard-export.xlsx",
            @"C:\DeskFlowDemo\Documents\Atlas\quarterly-risk-dashboard-export.xlsx",
            DocumentStatusNames.InReview,
            "manager@deskflow.ai",
            "Yonetim ozeti grafikleri kontrol bekliyor.");

        AddDocumentIfMissing(
            dbContext,
            riskReporting.Id,
            "risk-report-final-summary.pdf",
            @"C:\DeskFlowDemo\Documents\Atlas\risk-report-final-summary.pdf",
            DocumentStatusNames.Uploaded,
            "admin@deskflow.ai",
            "Final PDF onaydan once finans ekibiyle paylasilacak.");

        AddDocumentIfMissing(
            dbContext,
            dataMigration.Id,
            "legacy-migration-signoff.pdf",
            @"C:\DeskFlowDemo\Documents\Atlas\legacy-migration-signoff.pdf",
            DocumentStatusNames.Approved,
            "manager@deskflow.ai",
            "Tamamlanan migration icin kapanis onayi.");

        AddDocumentIfMissing(
            dbContext,
            backoffice.Id,
            "ecommerce-backoffice-process-map.vsdx",
            @"C:\DeskFlowDemo\Documents\Nova\ecommerce-backoffice-process-map.vsdx",
            DocumentStatusNames.Uploaded,
            "admin@deskflow.ai",
            "Stok, iade ve alarm surecleri icin surec haritasi.");

        AddDocumentIfMissing(
            dbContext,
            intakeWorkflow.Id,
            "patient-intake-kvkk-form.docx",
            @"C:\DeskFlowDemo\Documents\Orion\patient-intake-kvkk-form.docx",
            DocumentStatusNames.InReview,
            "manager@deskflow.ai",
            "KVKK onay metni hukuk ekibi tarafindan inceleniyor.");

        AddDocumentIfMissing(
            dbContext,
            customerPortal.Id,
            "low-risk-handoff.txt",
            GetDemoDocumentPath("low-risk-handoff.txt"),
            DocumentStatusNames.Uploaded,
            "admin@deskflow.ai",
            "Development Mock AI low-risk demo belgesi.");

        AddDocumentIfMissing(
            dbContext,
            customerPortal.Id,
            "medium-risk-approval.txt",
            GetDemoDocumentPath("medium-risk-approval.txt"),
            DocumentStatusNames.InReview,
            "manager@deskflow.ai",
            "Development Mock AI medium-risk demo belgesi.");

        AddDocumentIfMissing(
            dbContext,
            warehouseApp.Id,
            "high-risk-delivery.txt",
            GetDemoDocumentPath("high-risk-delivery.txt"),
            DocumentStatusNames.NeedsUpdate,
            "manager@deskflow.ai",
            "Development Mock AI high-risk demo belgesi.");

        dbContext.SaveChanges();
    }

    private static string GetDemoDocumentPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "DemoDocuments", fileName);
    }

    private static void AddCustomerIfMissing(
        DeskFlowDbContext dbContext,
        string email,
        string companyName,
        string contactName,
        string status)
    {
        if (dbContext.Customers.Any(customer => customer.Email == email))
        {
            return;
        }

        dbContext.Customers.Add(new Customer(companyName, contactName, email, status));
    }

    private static void AddEmployeeIfMissing(
        DeskFlowDbContext dbContext,
        string email,
        string fullName,
        string department,
        string roleTitle,
        string availabilityStatus,
        DateTime? leaveStart,
        DateTime? leaveEnd,
        string skills,
        string backupEmployeeName)
    {
        if (dbContext.Employees.Any(employee => employee.Email == email))
        {
            return;
        }

        dbContext.Employees.Add(new Employee(
            fullName,
            email,
            department,
            roleTitle,
            availabilityStatus,
            leaveStart,
            leaveEnd,
            skills,
            backupEmployeeName));
    }

    private static void AddUserAccountIfMissing(
        DeskFlowDbContext dbContext,
        string email,
        string password,
        string role,
        int? employeeId)
    {
        if (dbContext.UserAccounts.Any(user => user.Email == email))
        {
            return;
        }

        dbContext.UserAccounts.Add(new UserAccount(
            email,
            DemoPasswordHasher.Hash(password),
            role,
            employeeId,
            isActive: true));
    }

    private static void AddProjectIfMissing(
        DeskFlowDbContext dbContext,
        int customerId,
        string name,
        string status,
        DateTime? dueDate)
    {
        if (dbContext.Projects.Any(project => project.Name == name))
        {
            return;
        }

        dbContext.Projects.Add(new WorkProject(customerId, name, status, dueDate));
    }

    private static void AddTaskIfMissing(
        DeskFlowDbContext dbContext,
        int projectId,
        string title,
        string status,
        string priority,
        DateTime? dueDate,
        string blockedBy = "")
    {
        if (dbContext.Tasks.Any(task => task.ProjectId == projectId && task.Title == title))
        {
            return;
        }

        dbContext.Tasks.Add(new WorkTask(projectId, title, status, priority, dueDate, blockedBy: blockedBy));
    }

    private static void RenameTaskIfExists(DeskFlowDbContext dbContext, string oldTitle, string newTitle)
    {
        WorkTask? task = dbContext.Tasks.FirstOrDefault(task => task.Title == oldTitle);

        if (task is null || dbContext.Tasks.Any(existingTask => existingTask.Title == newTitle))
        {
            return;
        }

        task.Rename(newTitle);
    }

    private static void AddDocumentIfMissing(
        DeskFlowDbContext dbContext,
        int projectId,
        string fileName,
        string filePath,
        string status,
        string uploadedByEmail,
        string notes)
    {
        if (dbContext.ProjectDocuments.Any(document => document.ProjectId == projectId && document.FileName == fileName))
        {
            return;
        }

        dbContext.ProjectDocuments.Add(new ProjectDocument(
            projectId,
            fileName,
            filePath,
            status,
            uploadedByEmail,
            notes));
    }

    private static void AssignTaskIfUnassigned(DeskFlowDbContext dbContext, string title, int employeeId)
    {
        WorkTask? task = dbContext.Tasks.FirstOrDefault(task => task.Title == title);

        if (task is null || task.AssignedEmployeeId.HasValue)
        {
            return;
        }

        task.ChangeWorkflow(task.Status, task.Priority, task.DueDate, employeeId);
    }

    private static Customer GetCustomer(DeskFlowDbContext dbContext, string email)
    {
        return dbContext.Customers.First(customer => customer.Email == email);
    }

    private static WorkProject GetProject(DeskFlowDbContext dbContext, string name)
    {
        return dbContext.Projects.First(project => project.Name == name);
    }

    private static Employee GetEmployee(DeskFlowDbContext dbContext, string email)
    {
        return dbContext.Employees.First(employee => employee.Email == email);
    }
}
