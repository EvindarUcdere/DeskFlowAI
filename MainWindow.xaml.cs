using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DeskFlowAI.Data;
using DeskFlowAI.Models;
using DeskFlowAI.Services;

namespace DeskFlowAI;

public partial class MainWindow : Window
{
    private readonly DemoAuthService _authService = new();
    private readonly DemoDashboardService _dashboardService = new();
    private readonly DemoCustomerService _customerService = new();
    private readonly DemoProjectService _projectService = new();
    private readonly DemoTaskService _taskService = new();
    private readonly DemoProjectDocumentService _documentService = new();
    private readonly DemoEmployeeService _employeeService = new();
    private readonly DemoUserAccountService _userAccountService = new();
    private readonly DemoAuditLogService _auditLogService = new();
    private readonly ObservableCollection<Customer> _customers = [];
    private readonly ObservableCollection<Customer> _filteredCustomers = [];
    private readonly ObservableCollection<WorkProject> _projects = [];
    private readonly ObservableCollection<WorkTask> _tasks = [];
    private readonly ObservableCollection<ProjectDocument> _documents = [];
    private readonly ObservableCollection<Employee> _employees = [];
    private readonly ObservableCollection<UserAccount> _userAccounts = [];
    private readonly ObservableCollection<WorkProject> _allProjects = [];
    private readonly ObservableCollection<WorkProject> _dueSoonProjects = [];
    private readonly ObservableCollection<AuditLogEntry> _auditLogs = [];
    private Customer? _selectedCustomer;
    private WorkProject? _selectedProject;
    private WorkTask? _selectedTask;
    private ProjectDocument? _selectedDocument;
    private Employee? _selectedEmployee;
    private UserAccount? _selectedUserAccount;
    private UserSession? _currentUser;

    public MainWindow()
    {
        InitializeComponent();
        new DatabaseInitializer().Initialize();
        LoadCustomers();
        LoadEmployees();
        LoadUserAccounts();
        LoadAuditLogs();
        ProjectsDataGrid.ItemsSource = _projects;
        TasksDataGrid.ItemsSource = _tasks;
        DocumentsDataGrid.ItemsSource = _documents;
        EmployeesDataGrid.ItemsSource = _employees;
        UserAccountsDataGrid.ItemsSource = _userAccounts;
        UserEmployeeComboBox.ItemsSource = _employees;
        TaskAssignedEmployeeComboBox.ItemsSource = _employees;
        AllProjectsDataGrid.ItemsSource = _allProjects;
        DueSoonProjectsDataGrid.ItemsSource = _dueSoonProjects;
        ProjectDueDatePicker.SelectedDate = DateTime.Today.AddDays(14);
        TaskDueDatePicker.SelectedDate = DateTime.Today.AddDays(7);
        LoadProjectsForSelectedCustomer();
        RefreshProjectOverview();
        UpdateProjectActionState();
        UpdateTaskActionState();
    }

    private void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        TrySignIn();
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TrySignIn();
        }
    }

    private void SignOutButton_Click(object sender, RoutedEventArgs e)
    {
        _currentUser = null;
        PasswordBox.Clear();
        DashboardView.Visibility = Visibility.Collapsed;
        LoginView.Visibility = Visibility.Visible;
        EmailTextBox.Focus();
    }

    private void TrySignIn()
    {
        string email = EmailTextBox.Text.Trim();
        string password = PasswordBox.Password;

        AuthResult result = _authService.SignIn(email, password);

        if (result.IsSuccess && result.User is not null)
        {
            ShowDashboard(result.User);
            return;
        }

        LoginErrorTextBlock.Text = result.ErrorMessage;
        LoginErrorTextBlock.Visibility = Visibility.Visible;
    }

    private void ShowDashboard(UserSession user)
    {
        _currentUser = user;
        LoginErrorTextBlock.Visibility = Visibility.Collapsed;
        SignedInUserTextBlock.Text = $"{user.FullName} | {user.Role} | {user.Email}";
        ApplyCustomerPermissions(user);
        ApplyProjectPermissions(user);
        ApplyTaskPermissions(user);
        ApplyDocumentPermissions(user);
        ApplyEmployeePermissions(user);
        ApplyUserManagementPermissions(user);
        ApplyWorkspaceForRole(user);
        RefreshDashboardSummary();
        LoginView.Visibility = Visibility.Collapsed;
        DashboardView.Visibility = Visibility.Visible;
    }

    private void RefreshDashboardSummary()
    {
        if (_currentUser is null)
        {
            return;
        }

        DashboardSummary summary = _dashboardService.GetSummaryFor(_currentUser);
        ActiveProjectsTextBlock.Text = summary.ActiveProjects.ToString();
        OpenTasksTextBlock.Text = summary.OpenTasks.ToString();
        OverdueTasksTextBlock.Text = summary.OverdueTasks.ToString();
        PendingAiDocumentsTextBlock.Text = summary.PendingAiDocuments.ToString();
    }

    private void LoadCustomers()
    {
        _customers.Clear();

        foreach (Customer customer in _customerService.GetCustomers())
        {
            _customers.Add(customer);
        }

        ApplyCustomerFilter();
        CustomersDataGrid.ItemsSource = _filteredCustomers;
    }

    private void LoadAuditLogs()
    {
        _auditLogs.Clear();

        foreach (AuditLogEntry auditLog in _auditLogService.GetEntries())
        {
            _auditLogs.Add(auditLog);
        }

        AuditLogsDataGrid.ItemsSource = _auditLogs;
    }

    private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.CustomerCreate, "customer ekleme"))
        {
            return;
        }

        if (!TryReadCustomerForm(out string companyName, out string contactName, out string email))
        {
            return;
        }

        Customer customer = _customerService.CreateCustomer(companyName, contactName, email);
        _customers.Add(customer);
        ApplyCustomerFilter();
        CustomersDataGrid.SelectedItem = customer;
        ClearCustomerForm();
        RecordAudit("Created", "Customer", $"{customer.CompanyName} eklendi.");
        ShowCustomerFormMessage("Customer eklendi.", isError: false);
    }

    private void UpdateCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.CustomerUpdate, "customer guncelleme"))
        {
            return;
        }

        if (_selectedCustomer is null)
        {
            ShowCustomerFormMessage("Guncellemek icin listeden bir customer sec.", isError: true);
            return;
        }

        if (!TryReadCustomerForm(out string companyName, out string contactName, out string email))
        {
            return;
        }

        Customer updatedCustomer = _customerService.UpdateCustomer(_selectedCustomer, companyName, contactName, email);
        string changeDetails = BuildCustomerChangeDetails(_selectedCustomer, updatedCustomer);

        if (string.IsNullOrWhiteSpace(changeDetails))
        {
            ShowCustomerFormMessage("Guncellenecek bir degisiklik yok.", isError: true);
            return;
        }

        int selectedIndex = _customers.IndexOf(_selectedCustomer);

        if (selectedIndex >= 0)
        {
            _customers[selectedIndex] = updatedCustomer;
            ApplyCustomerFilter();
            CustomersDataGrid.SelectedItem = updatedCustomer;
            _selectedCustomer = updatedCustomer;
            RecordAudit("Updated", "Customer", $"{updatedCustomer.CompanyName}: {changeDetails}");
            ShowCustomerFormMessage("Selected customer guncellendi.", isError: false);
        }
    }

    private void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.CustomerDelete, "customer silme"))
        {
            return;
        }

        if (CustomersDataGrid.SelectedItem is not Customer selectedCustomer)
        {
            ShowCustomerFormMessage("Silmek icin listeden bir customer sec.", isError: true);
            return;
        }

        _customerService.DeleteCustomer(selectedCustomer);
        _customers.Remove(selectedCustomer);
        ApplyCustomerFilter();
        ClearCustomerForm();
        RecordAudit("Deleted", "Customer", $"{selectedCustomer.CompanyName} silindi.");
        ShowCustomerFormMessage("Selected customer silindi.", isError: false);
    }

    private void CustomerSearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        ApplyCustomerFilter();
    }

    private void ClearCustomerSearchButton_Click(object sender, RoutedEventArgs e)
    {
        CustomerSearchTextBox.Clear();
        ApplyCustomerFilter();
    }

    private void CustomersDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (CustomersDataGrid.SelectedItem is not Customer selectedCustomer)
        {
            return;
        }

        _selectedCustomer = selectedCustomer;
        CustomerFormTitleTextBlock.Text = "Edit customer";
        CustomerCompanyTextBox.Text = selectedCustomer.CompanyName;
        CustomerContactTextBox.Text = selectedCustomer.ContactName;
        CustomerEmailTextBox.Text = selectedCustomer.Email;
        CustomerFormMessageTextBlock.Visibility = Visibility.Collapsed;
        LoadProjectsForSelectedCustomer(selectFirstProject: true);
    }

    private void ProjectsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ProjectsDataGrid.SelectedItem is not WorkProject selectedProject)
        {
            _selectedProject = null;
            UpdateProjectActionState();
            return;
        }

        _selectedProject = selectedProject;
        ProjectNameTextBox.Text = selectedProject.Name;
        SelectProjectStatus(selectedProject.Status);
        ProjectDueDatePicker.SelectedDate = selectedProject.DueDate;
        ProjectFormMessageTextBlock.Visibility = Visibility.Collapsed;
        LoadTasksForSelectedProject(selectFirstTask: true);
        LoadDocumentsForCurrentContext(selectFirstDocument: true);
        UpdateProjectActionState();
    }

    private void TasksDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (TasksDataGrid.SelectedItem is not WorkTask selectedTask)
        {
            _selectedTask = null;
            UpdateTaskActionState();
            return;
        }

        _selectedTask = selectedTask;
        TaskTitleTextBox.Text = selectedTask.Title;
        SelectTaskStatus(selectedTask.Status);
        SelectTaskPriority(selectedTask.Priority);
        SelectAssignedEmployee(selectedTask.AssignedEmployeeId);
        TaskDueDatePicker.SelectedDate = selectedTask.DueDate;
        TaskFormMessageTextBlock.Visibility = Visibility.Collapsed;
        UpdateTaskActionState();
    }

    private void DocumentsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (DocumentsDataGrid.SelectedItem is not ProjectDocument selectedDocument)
        {
            _selectedDocument = null;
            UpdateDocumentActionState();
            return;
        }

        _selectedDocument = selectedDocument;
        DocumentFileNameTextBox.Text = selectedDocument.FileName;
        DocumentFilePathTextBox.Text = selectedDocument.FilePath;
        SelectDocumentStatus(selectedDocument.Status);
        DocumentNotesTextBox.Text = selectedDocument.Notes;
        PopulateDocumentAIFields(selectedDocument);
        DocumentFormMessageTextBlock.Visibility = Visibility.Collapsed;
        UpdateDocumentActionState();
    }

    private void EmployeesDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (EmployeesDataGrid.SelectedItem is not Employee selectedEmployee)
        {
            _selectedEmployee = null;
            UpdateEmployeeActionState();
            return;
        }

        _selectedEmployee = selectedEmployee;
        EmployeeFullNameTextBox.Text = selectedEmployee.FullName;
        EmployeeEmailTextBox.Text = selectedEmployee.Email;
        EmployeeDepartmentTextBox.Text = selectedEmployee.Department;
        EmployeeRoleTextBox.Text = selectedEmployee.RoleTitle;
        SelectEmployeeAvailability(selectedEmployee.AvailabilityStatus);
        EmployeeLeaveStartDatePicker.SelectedDate = selectedEmployee.LeaveStart;
        EmployeeLeaveEndDatePicker.SelectedDate = selectedEmployee.LeaveEnd;
        EmployeeSkillsTextBox.Text = selectedEmployee.Skills;
        EmployeeBackupTextBox.Text = selectedEmployee.BackupEmployeeName;
        EmployeeWorkloadSummaryTextBlock.Text = BuildEmployeeWorkloadSummary(selectedEmployee);
        EmployeeFormMessageTextBlock.Visibility = Visibility.Collapsed;
        UpdateEmployeeActionState();
    }

    private void UserAccountsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (UserAccountsDataGrid.SelectedItem is not UserAccount selectedUser)
        {
            _selectedUserAccount = null;
            UpdateUserAccountActionState();
            return;
        }

        _selectedUserAccount = selectedUser;
        UserEmailTextBox.Text = selectedUser.Email;
        SelectUserRole(selectedUser.Role);
        SelectUserEmployee(selectedUser.EmployeeId);
        UserIsActiveCheckBox.IsChecked = selectedUser.IsActive;
        NewUserPasswordBox.Clear();
        UserFormMessageTextBlock.Visibility = Visibility.Collapsed;
        UpdateUserAccountActionState();
    }

    private void TaskFilter_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        LoadTasksForCurrentContext(selectFirstTask: true);
    }

    private void ClearCustomerFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearCustomerForm();
    }

    private void ClearCustomerForm()
    {
        _selectedCustomer = null;
        _selectedProject = null;
        _selectedTask = null;
        _selectedDocument = null;
        CustomersDataGrid.SelectedItem = null;
        ProjectsDataGrid.SelectedItem = null;
        TasksDataGrid.SelectedItem = null;
        DocumentsDataGrid.SelectedItem = null;
        CustomerFormTitleTextBlock.Text = "Add customer";
        CustomerCompanyTextBox.Clear();
        CustomerContactTextBox.Clear();
        CustomerEmailTextBox.Clear();
        CustomerFormMessageTextBlock.Visibility = Visibility.Collapsed;
        ProjectNameTextBox.Clear();
        ProjectDueDatePicker.SelectedDate = DateTime.Today.AddDays(14);
        ProjectFormMessageTextBlock.Visibility = Visibility.Collapsed;
        TaskTitleTextBox.Clear();
        SelectAssignedEmployee(null);
        TaskDueDatePicker.SelectedDate = DateTime.Today.AddDays(7);
        TaskFormMessageTextBlock.Visibility = Visibility.Collapsed;
        ClearDocumentForm();
        _tasks.Clear();
        _documents.Clear();
        SelectedProjectForTaskTextBlock.Text = "Once Projects sekmesinden bir project sec.";
        TasksEmptyTextBlock.Visibility = Visibility.Collapsed;
        SelectedProjectForDocumentTextBlock.Text = "Once Projects sekmesinden bir project sec.";
        DocumentsEmptyTextBlock.Visibility = Visibility.Collapsed;
        LoadProjectsForSelectedCustomer();
        UpdateProjectActionState();
        UpdateTaskActionState();
        UpdateDocumentActionState();
        CustomerCompanyTextBox.Focus();
    }

    private void AddProjectButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.ProjectCreate, "proje ekleme"))
        {
            return;
        }

        if (_selectedCustomer is null)
        {
            ShowProjectFormMessage("Proje eklemek icin once bir customer sec.", isError: true);
            return;
        }

        string projectName = ProjectNameTextBox.Text.Trim();
        string status = GetSelectedProjectStatus();
        DateTime? dueDate = ProjectDueDatePicker.SelectedDate;

        if (string.IsNullOrWhiteSpace(projectName))
        {
            ShowProjectFormMessage("Project name zorunludur.", isError: true);
            return;
        }

        WorkProject project = _projectService.CreateProject(_selectedCustomer.Id, projectName, status, dueDate);
        LoadProjectsForSelectedCustomer(project.Id);
        RefreshDashboardSummary();
        RefreshProjectOverview();
        string dueDateText = dueDate.HasValue ? dueDate.Value.ToString("dd.MM.yyyy") : "teslim tarihi yok";
        RecordAudit("Created", "Project", $"{project.Name} projesi {_selectedCustomer.CompanyName} icin {project.Status} status ile eklendi. Due: {dueDateText}.");
        ShowProjectFormMessage("Project eklendi ve listede secildi.", isError: false);
    }

    private void UpdateProjectStatusButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.ProjectUpdate, "proje guncelleme"))
        {
            return;
        }

        if (_selectedProject is null)
        {
            ShowProjectFormMessage("Status guncellemek icin listeden bir project sec.", isError: true);
            return;
        }

        string newStatus = GetSelectedProjectStatus();

        if (_selectedProject.Status == newStatus)
        {
            ShowProjectFormMessage("Project status zaten bu degerde.", isError: true);
            return;
        }

        string oldStatus = _selectedProject.Status;
        WorkProject updatedProject = _projectService.UpdateProjectStatus(_selectedProject, newStatus);
        int selectedIndex = _projects.IndexOf(_selectedProject);

        if (selectedIndex >= 0)
        {
            _projects[selectedIndex] = updatedProject;
        }

        SelectProject(updatedProject);
        RefreshDashboardSummary();
        RefreshProjectOverview();
        RecordAudit("Updated", "Project", $"{updatedProject.Name}: Status '{oldStatus}' -> '{newStatus}'");
        ShowProjectFormMessage("Project status guncellendi.", isError: false);
    }

    private void AddTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.TaskCreate, "gorev ekleme"))
        {
            return;
        }

        if (_selectedProject is null)
        {
            ShowTaskFormMessage("Gorev eklemek icin once bir project sec.", isError: true);
            return;
        }

        string title = TaskTitleTextBox.Text.Trim();
        string status = GetSelectedTaskStatus();
        string priority = GetSelectedTaskPriority();
        int? assignedEmployeeId = GetSelectedAssignedEmployeeId();
        DateTime? dueDate = TaskDueDatePicker.SelectedDate;

        if (string.IsNullOrWhiteSpace(title))
        {
            ShowTaskFormMessage("Task title zorunludur.", isError: true);
            return;
        }

        Employee? assignedEmployee = FindEmployee(assignedEmployeeId);
        WorkTask task = _taskService.CreateTask(_selectedProject.Id, title, status, priority, dueDate, assignedEmployeeId);
        LoadTasksForSelectedProject(task.Id);
        RefreshDashboardSummary();
        string dueDateText = dueDate.HasValue ? dueDate.Value.ToString("dd.MM.yyyy") : "teslim tarihi yok";
        string assignedText = assignedEmployee is null ? "atanan kisi yok" : assignedEmployee.FullName;
        RecordAudit("Created", "Task", $"{task.Title} gorevi {_selectedProject.Name} projesine {priority} priority ile eklendi. Assigned: {assignedText}. Due: {dueDateText}.");
        string createMessage = TaskIsVisibleInCurrentFilters(task)
            ? "Task eklendi ve listede secildi."
            : "Task eklendi. Aktif filtreler nedeniyle listede gorunmuyor.";
        ShowTaskFormMessage(AppendAssignmentWarning(createMessage, assignedEmployee), isError: false);
    }

    private void UpdateTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.TaskUpdate, "gorev guncelleme"))
        {
            return;
        }

        if (_selectedTask is null)
        {
            ShowTaskFormMessage("Guncellemek icin listeden bir task sec.", isError: true);
            return;
        }

        string newStatus = GetSelectedTaskStatus();
        string newPriority = GetSelectedTaskPriority();
        int? newAssignedEmployeeId = GetSelectedAssignedEmployeeId();
        DateTime? newDueDate = TaskDueDatePicker.SelectedDate;

        string oldStatus = _selectedTask.Status;
        string oldPriority = _selectedTask.Priority;
        string oldAssignedEmployeeName = GetEmployeeName(_selectedTask.AssignedEmployeeId);
        Employee? newAssignedEmployee = FindEmployee(newAssignedEmployeeId);
        string newAssignedEmployeeName = GetEmployeeName(newAssignedEmployeeId);
        DateTime? oldDueDate = _selectedTask.DueDate;

        WorkTask updatedTask = _taskService.UpdateTaskWorkflow(_selectedTask, newStatus, newPriority, newDueDate, newAssignedEmployeeId);
        LoadTasksForCurrentContext(updatedTask.Id);
        RefreshDashboardSummary();

        string changeDetails = BuildTaskChangeDetails(oldStatus, newStatus, oldPriority, newPriority, oldAssignedEmployeeName, newAssignedEmployeeName, oldDueDate, newDueDate);
        RecordAudit("Updated", "Task", $"{updatedTask.Title}: {changeDetails}");
        string updateMessage = TaskIsVisibleInCurrentFilters(updatedTask)
            ? "Task guncellendi."
            : "Task guncellendi. Aktif filtreler nedeniyle listede gorunmuyor.";
        ShowTaskFormMessage(AppendAssignmentWarning(updateMessage, newAssignedEmployee), isError: false);
    }

    private void MarkTaskDoneButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.TaskUpdate, "gorev tamamlama"))
        {
            return;
        }

        if (_selectedTask is null)
        {
            ShowTaskFormMessage("Tamamlamak icin listeden bir task sec.", isError: true);
            return;
        }

        if (_selectedTask.Status == TaskStatusNames.Done)
        {
            ShowTaskFormMessage("Bu task zaten Done durumunda.", isError: true);
            return;
        }

        string oldStatus = _selectedTask.Status;
        WorkTask updatedTask = _taskService.UpdateTaskWorkflow(
            _selectedTask,
            TaskStatusNames.Done,
            _selectedTask.Priority,
            _selectedTask.DueDate,
            _selectedTask.AssignedEmployeeId);

        LoadTasksForCurrentContext(updatedTask.Id);
        RefreshDashboardSummary();
        RecordAudit("Updated", "Task", $"{updatedTask.Title}: Status '{oldStatus}' -> '{TaskStatusNames.Done}'");
        ShowTaskFormMessage(TaskIsVisibleInCurrentFilters(updatedTask)
            ? "Task Done olarak isaretlendi."
            : "Task Done olarak isaretlendi. Aktif filtreler nedeniyle listede gorunmuyor.", isError: false);
    }

    private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.EmployeeManage, "calisan yonetimi"))
        {
            return;
        }

        if (!TryReadEmployeeForm(
            out string fullName,
            out string email,
            out string department,
            out string roleTitle,
            out string availability,
            out DateTime? leaveStart,
            out DateTime? leaveEnd,
            out string skills,
            out string backupEmployeeName))
        {
            return;
        }

        Employee employee = _employeeService.CreateEmployee(
            fullName,
            email,
            department,
            roleTitle,
            availability,
            leaveStart,
            leaveEnd,
            skills,
            backupEmployeeName);

        LoadEmployees(employee.Id);
        RecordAudit("Created", "Employee", $"{employee.FullName} calisan kaydi {department} ekibine eklendi.");
        ShowEmployeeFormMessage("Calisan eklendi ve listede secildi.", isError: false);
    }

    private void UpdateEmployeeButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.EmployeeManage, "calisan yonetimi"))
        {
            return;
        }

        if (_selectedEmployee is null)
        {
            ShowEmployeeFormMessage("Guncellemek icin listeden bir calisan sec.", isError: true);
            return;
        }

        if (!TryReadEmployeeForm(
            out string fullName,
            out string email,
            out string department,
            out string roleTitle,
            out string availability,
            out DateTime? leaveStart,
            out DateTime? leaveEnd,
            out string skills,
            out string backupEmployeeName))
        {
            return;
        }

        string changeDetails = BuildEmployeeChangeDetails(_selectedEmployee, fullName, email, department, roleTitle, availability, leaveStart, leaveEnd, skills, backupEmployeeName);
        Employee employee = _employeeService.UpdateEmployee(
            _selectedEmployee,
            fullName,
            email,
            department,
            roleTitle,
            availability,
            leaveStart,
            leaveEnd,
            skills,
            backupEmployeeName);

        LoadEmployees(employee.Id);
        RecordAudit("Updated", "Employee", $"{employee.FullName}: {changeDetails}");
        ShowEmployeeFormMessage("Calisan bilgileri guncellendi.", isError: false);
    }

    private void ClearEmployeeFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearEmployeeForm();
    }

    private void AddUserButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.UserManage, "kullanici yonetimi"))
        {
            return;
        }

        if (!TryReadUserForm(requirePassword: true, out string email, out string password, out string role, out int? employeeId, out bool isActive))
        {
            return;
        }

        try
        {
            UserAccount user = _userAccountService.CreateUser(email, password, role, employeeId, isActive);
            LoadUserAccounts(user.Id);
            RecordAudit("Created", "User", $"{user.Email} kullanicisi {role} rolu ile olusturuldu.");
            ShowUserFormMessage("Kullanici olusturuldu ve listede secildi.", isError: false);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            ShowUserFormMessage("Bu email veya calisan baglantisi zaten baska bir kullanicida kullaniliyor.", isError: true);
        }
    }

    private void UpdateUserButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.UserManage, "kullanici yonetimi"))
        {
            return;
        }

        if (_selectedUserAccount is null)
        {
            ShowUserFormMessage("Guncellemek icin listeden bir kullanici sec.", isError: true);
            return;
        }

        if (!TryReadUserForm(requirePassword: false, out string email, out _, out string role, out int? employeeId, out bool isActive))
        {
            return;
        }

        try
        {
            string oldEmail = _selectedUserAccount.Email;
            string oldRole = _selectedUserAccount.Role;
            UserAccount user = _userAccountService.UpdateUser(_selectedUserAccount, email, role, employeeId, isActive);
            LoadUserAccounts(user.Id);
            RecordAudit("Updated", "User", $"{oldEmail}: Role '{oldRole}' -> '{role}', Active: {isActive}");
            ShowUserFormMessage("Kullanici bilgileri guncellendi.", isError: false);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            ShowUserFormMessage("Bu email veya calisan baglantisi zaten baska bir kullanicida kullaniliyor.", isError: true);
        }
    }

    private void ResetUserPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.UserManage, "sifre resetleme"))
        {
            return;
        }

        if (_selectedUserAccount is null)
        {
            ShowUserFormMessage("Sifre resetlemek icin listeden bir kullanici sec.", isError: true);
            return;
        }

        string password = NewUserPasswordBox.Password.Trim();

        if (password.Length < 6)
        {
            ShowUserFormMessage("Yeni sifre en az 6 karakter olmalidir.", isError: true);
            return;
        }

        UserAccount user = _userAccountService.ResetPassword(_selectedUserAccount, password);
        LoadUserAccounts(user.Id);
        NewUserPasswordBox.Clear();
        RecordAudit("Updated", "User", $"{user.Email} sifresi resetlendi.");
        ShowUserFormMessage("Kullanici sifresi resetlendi.", isError: false);
    }

    private void ClearUserFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearUserForm();
    }

    private void AddDocumentButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.DocumentCreate, "belge ekleme"))
        {
            return;
        }

        if (_selectedProject is null)
        {
            ShowDocumentFormMessage("Belge eklemek icin once bir project sec.", isError: true);
            return;
        }

        if (!TryReadDocumentForm(out string fileName, out string filePath, out string status, out string notes))
        {
            return;
        }

        string uploadedByEmail = _currentUser?.Email ?? "unknown";
        ProjectDocument document = _documentService.CreateDocument(_selectedProject.Id, fileName, filePath, status, uploadedByEmail, notes);
        LoadDocumentsForCurrentContext(document.Id);
        RefreshDashboardSummary();
        RecordAudit("Created", "Document", $"{fileName} belgesi {_selectedProject.Name} projesine {uploadedByEmail} tarafindan eklendi.");
        ShowDocumentFormMessage("Belge eklendi ve listede secildi.", isError: false);
    }

    private void UpdateDocumentStatusButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.DocumentUpdate, "belge guncelleme"))
        {
            return;
        }

        if (_selectedDocument is null)
        {
            ShowDocumentFormMessage("Guncellemek icin listeden bir belge sec.", isError: true);
            return;
        }

        string newStatus = GetSelectedDocumentStatus();
        string notes = DocumentNotesTextBox.Text.Trim();
        string oldStatus = _selectedDocument.Status;

        ProjectDocument document = _documentService.UpdateDocumentStatus(_selectedDocument, newStatus, notes);
        LoadDocumentsForCurrentContext(document.Id);
        RecordAudit("Updated", "Document", $"{document.FileName}: Status '{oldStatus}' -> '{newStatus}'");
        ShowDocumentFormMessage("Belge status bilgisi guncellendi.", isError: false);
    }

    private void AnalyzeDocumentButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsurePermission(PermissionNames.DocumentUpdate, "belge analizi"))
        {
            return;
        }

        if (_selectedDocument is null)
        {
            ShowDocumentFormMessage("Analiz etmek icin listeden bir belge sec.", isError: true);
            return;
        }

        string oldAIStatus = _selectedDocument.AIAnalysisStatus;
        ProjectDocument document = _documentService.AnalyzeDocument(_selectedDocument);
        LoadDocumentsForCurrentContext(document.Id);
        RefreshDashboardSummary();
        RecordAudit("Analyzed", "Document", BuildDocumentAnalysisAuditDetails(document, oldAIStatus));
        ShowDocumentFormMessage("Demo AI analizi olusturuldu.", isError: false);
    }

    private void ClearDocumentFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearDocumentForm();
    }

    private bool TryReadCustomerForm(out string companyName, out string contactName, out string email)
    {
        companyName = CustomerCompanyTextBox.Text.Trim();
        contactName = CustomerContactTextBox.Text.Trim();
        email = CustomerEmailTextBox.Text.Trim();

        if (!string.IsNullOrWhiteSpace(companyName)
            && !string.IsNullOrWhiteSpace(contactName)
            && !string.IsNullOrWhiteSpace(email))
        {
            return true;
        }

        ShowCustomerFormMessage("Company, contact ve email alanlari zorunludur.", isError: true);
        return false;
    }

    private void ApplyCustomerFilter()
    {
        string searchTerm = CustomerSearchTextBox.Text.Trim();
        _filteredCustomers.Clear();

        IEnumerable<Customer> query = _customers;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(customer =>
                customer.CompanyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || customer.ContactName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || customer.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || customer.Status.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        foreach (Customer customer in query)
        {
            _filteredCustomers.Add(customer);
        }
    }

    private void ShowCustomerFormMessage(string message, bool isError)
    {
        CustomerFormMessageTextBlock.Text = message;
        CustomerFormMessageTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.SeaGreen;
        CustomerFormMessageTextBlock.Visibility = Visibility.Visible;
    }

    private void ApplyCustomerPermissions(UserSession user)
    {
        AddCustomerButton.IsEnabled = user.HasPermission(PermissionNames.CustomerCreate);
        UpdateCustomerButton.IsEnabled = user.HasPermission(PermissionNames.CustomerUpdate);
        DeleteCustomerButton.IsEnabled = user.HasPermission(PermissionNames.CustomerDelete);
    }

    private void ApplyProjectPermissions(UserSession user)
    {
        AddProjectButton.IsEnabled = user.HasPermission(PermissionNames.ProjectCreate);
        UpdateProjectActionState();
    }

    private void ApplyTaskPermissions(UserSession user)
    {
        AddTaskButton.IsEnabled = user.HasPermission(PermissionNames.TaskCreate)
            && user.Role != RoleNames.Staff;
        UpdateTaskActionState();
    }

    private void ApplyDocumentPermissions(UserSession user)
    {
        AddDocumentButton.IsEnabled = user.HasPermission(PermissionNames.DocumentCreate);
        UpdateDocumentActionState();
    }

    private void ApplyEmployeePermissions(UserSession user)
    {
        bool canManageEmployees = user.HasPermission(PermissionNames.EmployeeManage);
        TeamTab.Visibility = canManageEmployees ? Visibility.Visible : Visibility.Collapsed;
        AddEmployeeButton.IsEnabled = canManageEmployees;
        UpdateEmployeeActionState();
    }

    private void ApplyUserManagementPermissions(UserSession user)
    {
        bool canManageUsers = user.HasPermission(PermissionNames.UserManage);
        UsersTab.Visibility = canManageUsers ? Visibility.Visible : Visibility.Collapsed;
        AddUserButton.IsEnabled = canManageUsers;
        UpdateUserAccountActionState();
    }

    private void ApplyWorkspaceForRole(UserSession user)
    {
        bool isStaff = user.Role == RoleNames.Staff;

        CustomerWorkspaceBorder.Visibility = isStaff ? Visibility.Collapsed : Visibility.Visible;
        MainWorkspaceGridSplitter.Visibility = isStaff ? Visibility.Collapsed : Visibility.Visible;
        CustomerTab.Visibility = isStaff ? Visibility.Collapsed : Visibility.Visible;
        ProjectsTab.Visibility = isStaff ? Visibility.Collapsed : Visibility.Visible;
        OverviewTab.Visibility = isStaff ? Visibility.Collapsed : Visibility.Visible;

        if (isStaff)
        {
            LeftWorkspaceColumn.MinWidth = 0;
            LeftWorkspaceColumn.Width = new GridLength(0);
            MainSplitterColumn.Width = new GridLength(0);
            RightWorkspaceColumn.Width = new GridLength(1, GridUnitType.Star);
            WorkspaceTabControl.SelectedItem = TasksTab;
            ConfigureTaskWorkspaceForStaff(user);
            LoadMyTasksForCurrentUser(selectFirstTask: true);
            LoadDocumentsForCurrentContext(selectFirstDocument: true);
            return;
        }

        LeftWorkspaceColumn.MinWidth = 360;
        LeftWorkspaceColumn.Width = new GridLength(1.15, GridUnitType.Star);
        MainSplitterColumn.Width = new GridLength(10);
        RightWorkspaceColumn.Width = new GridLength(1, GridUnitType.Star);
        ConfigureTaskWorkspaceForOperations();
    }

    private void ConfigureTaskWorkspaceForStaff(UserSession user)
    {
        string fullName = user.FullName;
        TaskListTitleTextBlock.Text = "My tasks";
        TaskListDescriptionTextBlock.Text = "Sana atanmis gorevler teslim tarihine ve oncelige gore siralanir.";
        SelectedProjectForTaskTextBlock.Text = $"{fullName} icin atanmis tasklar.";
        TaskDetailsTitleTextBlock.Text = "My task details";
        TaskDetailsDescriptionTextBlock.Text = "Secili gorevin durumunu, onceligini ve teslim tarihini guncelle.";
        AddTaskButton.IsEnabled = false;
        TaskAssignedEmployeeComboBox.IsEnabled = false;
    }

    private void ConfigureTaskWorkspaceForOperations()
    {
        TaskListTitleTextBlock.Text = "Task list";
        TaskListDescriptionTextBlock.Text = "Secili project icindeki gorevler teslim tarihine ve oncelige gore siralanir.";
        TaskDetailsTitleTextBlock.Text = "Task details";
        TaskDetailsDescriptionTextBlock.Text = "Yeni gorev ekle veya secili gorevin is akis bilgilerini guncelle.";
        TaskAssignedEmployeeComboBox.IsEnabled = true;
    }

    private bool EnsurePermission(string permission, string operationName)
    {
        if (_currentUser?.HasPermission(permission) == true)
        {
            return true;
        }

        ShowCustomerFormMessage($"Bu kullanicinin {operationName} yetkisi yok.", isError: true);
        return false;
    }

    private void RecordAudit(string action, string entityName, string details)
    {
        if (_currentUser is null)
        {
            return;
        }

        AuditLogEntry entry = _auditLogService.CreateEntry(_currentUser, action, entityName, details);
        _auditLogs.Insert(0, entry);
    }

    private static string BuildCustomerChangeDetails(Customer oldCustomer, Customer newCustomer)
    {
        List<string> changes = [];

        AddChangeIfNeeded(changes, "Company", oldCustomer.CompanyName, newCustomer.CompanyName);
        AddChangeIfNeeded(changes, "Contact", oldCustomer.ContactName, newCustomer.ContactName);
        AddChangeIfNeeded(changes, "Email", oldCustomer.Email, newCustomer.Email);
        AddChangeIfNeeded(changes, "Status", oldCustomer.Status, newCustomer.Status);

        return string.Join("; ", changes);
    }

    private static void AddChangeIfNeeded(List<string> changes, string fieldName, string oldValue, string newValue)
    {
        if (oldValue.Equals(newValue, StringComparison.Ordinal))
        {
            return;
        }

        changes.Add($"{fieldName}: '{oldValue}' -> '{newValue}'");
    }

    private static string BuildTaskChangeDetails(
        string oldStatus,
        string newStatus,
        string oldPriority,
        string newPriority,
        string oldAssignedEmployeeName,
        string newAssignedEmployeeName,
        DateTime? oldDueDate,
        DateTime? newDueDate)
    {
        List<string> changes = [];

        AddChangeIfNeeded(changes, "Status", oldStatus, newStatus);
        AddChangeIfNeeded(changes, "Priority", oldPriority, newPriority);
        AddChangeIfNeeded(changes, "Assigned to", oldAssignedEmployeeName, newAssignedEmployeeName);
        AddChangeIfNeeded(changes, "Due date", FormatAuditDate(oldDueDate), FormatAuditDate(newDueDate));

        return changes.Count == 0
            ? "No workflow changes"
            : string.Join("; ", changes);
    }

    private static string FormatAuditDate(DateTime? date)
    {
        return date.HasValue ? date.Value.ToString("dd.MM.yyyy") : "none";
    }

    private static string BuildDocumentAnalysisAuditDetails(ProjectDocument document, string oldAIStatus)
    {
        string customerName = document.Project?.Customer?.CompanyName ?? "Unknown customer";
        string projectName = document.Project?.Name ?? "Unknown project";
        string analyzedAt = document.AnalyzedAt.HasValue
            ? document.AnalyzedAt.Value.ToString("dd.MM.yyyy HH:mm")
            : "none";

        return $"{document.FileName}: AI Status '{oldAIStatus}' -> '{document.AIAnalysisStatus}'. Customer: {customerName}. Project: {projectName}. Analyzed at: {analyzedAt}. Risk note: {document.AIRiskNotes}";
    }

    private static string BuildEmployeeChangeDetails(
        Employee oldEmployee,
        string fullName,
        string email,
        string department,
        string roleTitle,
        string availability,
        DateTime? leaveStart,
        DateTime? leaveEnd,
        string skills,
        string backupEmployeeName)
    {
        List<string> changes = [];

        AddChangeIfNeeded(changes, "Full name", oldEmployee.FullName, fullName);
        AddChangeIfNeeded(changes, "Email", oldEmployee.Email, email);
        AddChangeIfNeeded(changes, "Department", oldEmployee.Department, department);
        AddChangeIfNeeded(changes, "Role", oldEmployee.RoleTitle, roleTitle);
        AddChangeIfNeeded(changes, "Availability", oldEmployee.AvailabilityStatus, availability);
        AddChangeIfNeeded(changes, "Leave start", FormatAuditDate(oldEmployee.LeaveStart), FormatAuditDate(leaveStart));
        AddChangeIfNeeded(changes, "Leave end", FormatAuditDate(oldEmployee.LeaveEnd), FormatAuditDate(leaveEnd));
        AddChangeIfNeeded(changes, "Skills", oldEmployee.Skills, skills);
        AddChangeIfNeeded(changes, "Backup", oldEmployee.BackupEmployeeName, backupEmployeeName);

        return changes.Count == 0
            ? "No employee changes"
            : string.Join("; ", changes);
    }

    private void LoadProjectsForSelectedCustomer(int? projectIdToSelect = null, bool selectFirstProject = false)
    {
        _projects.Clear();
        _selectedProject = null;
        _selectedTask = null;
        _selectedDocument = null;
        ProjectsDataGrid.SelectedItem = null;
        TasksDataGrid.SelectedItem = null;
        DocumentsDataGrid.SelectedItem = null;
        _tasks.Clear();
        _documents.Clear();
        SelectedProjectForTaskTextBlock.Text = "Once Projects sekmesinden bir project sec.";
        TasksEmptyTextBlock.Visibility = Visibility.Collapsed;
        SelectedProjectForDocumentTextBlock.Text = "Once Projects sekmesinden bir project sec.";
        DocumentsEmptyTextBlock.Visibility = Visibility.Collapsed;
        UpdateProjectActionState();
        UpdateTaskActionState();
        UpdateDocumentActionState();

        IEnumerable<WorkProject> projects = _selectedCustomer is null
            ? _projectService.GetAllProjects()
            : _projectService.GetProjectsForCustomer(_selectedCustomer.Id);

        SelectedCustomerForProjectTextBlock.Text = _selectedCustomer is null
            ? "All projects: customer secilmedigi icin tum projeler listeleniyor."
            : $"Selected customer: {_selectedCustomer.CompanyName}";

        foreach (WorkProject project in projects)
        {
            _projects.Add(project);
        }

        ProjectsEmptyTextBlock.Text = _selectedCustomer is null
            ? "Henuz hic proje yok."
            : "Bu customer icin henuz proje yok.";
        ProjectsEmptyTextBlock.Visibility = _projects.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        if (projectIdToSelect is null && selectFirstProject && _projects.Count > 0)
        {
            projectIdToSelect = _projects[0].Id;
        }

        if (projectIdToSelect is null)
        {
            ProjectNameTextBox.Clear();
            SelectProjectStatus(ProjectStatusNames.Planning);
            ProjectDueDatePicker.SelectedDate = DateTime.Today.AddDays(14);
            TaskTitleTextBox.Clear();
            TaskDueDatePicker.SelectedDate = DateTime.Today.AddDays(7);
            ClearDocumentForm();
            return;
        }

        WorkProject? projectToSelect = _projects.FirstOrDefault(project => project.Id == projectIdToSelect.Value);

        if (projectToSelect is not null)
        {
            SelectProject(projectToSelect);
        }
    }

    private void SelectProject(WorkProject project)
    {
        _selectedProject = project;
        ProjectsDataGrid.SelectedItem = project;
        ProjectsDataGrid.ScrollIntoView(project);
        ProjectNameTextBox.Text = project.Name;
        SelectProjectStatus(project.Status);
        ProjectDueDatePicker.SelectedDate = project.DueDate;
        LoadTasksForSelectedProject();
        LoadDocumentsForCurrentContext();
        UpdateProjectActionState();
    }

    private void LoadTasksForCurrentContext(int? taskIdToSelect = null, bool selectFirstTask = false)
    {
        if (IsStaffUser())
        {
            LoadMyTasksForCurrentUser(taskIdToSelect, selectFirstTask);
            return;
        }

        LoadTasksForSelectedProject(taskIdToSelect, selectFirstTask);
    }

    private void LoadTasksForSelectedProject(int? taskIdToSelect = null, bool selectFirstTask = false)
    {
        _tasks.Clear();
        _selectedTask = null;
        TasksDataGrid.SelectedItem = null;
        UpdateTaskActionState();

        if (_selectedProject is null)
        {
            SelectedProjectForTaskTextBlock.Text = "Once Projects sekmesinden bir project sec.";
            TasksEmptyTextBlock.Visibility = Visibility.Collapsed;
            return;
        }

        SelectedProjectForTaskTextBlock.Text = $"Selected project: {_selectedProject.Name}";

        foreach (WorkTask task in GetFilteredTasksForCurrentContext())
        {
            _tasks.Add(task);
        }

        TasksEmptyTextBlock.Visibility = _tasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TasksEmptyTextBlock.Text = GetTaskEmptyMessage();

        if (taskIdToSelect is null && selectFirstTask && _tasks.Count > 0)
        {
            taskIdToSelect = _tasks[0].Id;
        }

        if (taskIdToSelect is null)
        {
            ResetTaskForm();
            return;
        }

        WorkTask? taskToSelect = _tasks.FirstOrDefault(task => task.Id == taskIdToSelect.Value);

        if (taskToSelect is not null)
        {
            SelectTask(taskToSelect);
            return;
        }

        ResetTaskForm();
    }

    private void LoadMyTasksForCurrentUser(int? taskIdToSelect = null, bool selectFirstTask = false)
    {
        _tasks.Clear();
        _selectedProject = null;
        _selectedTask = null;
        TasksDataGrid.SelectedItem = null;
        UpdateTaskActionState();

        if (_currentUser?.EmployeeId is null)
        {
            SelectedProjectForTaskTextBlock.Text = "Bu kullanici bir employee kaydina bagli degil.";
            TasksEmptyTextBlock.Text = "Employee baglantisi olmadigi icin task listelenemiyor.";
            TasksEmptyTextBlock.Visibility = Visibility.Visible;
            ResetTaskForm();
            return;
        }

        SelectedProjectForTaskTextBlock.Text = $"{_currentUser.FullName} icin atanmis tasklar.";

        foreach (WorkTask task in GetFilteredTasksForCurrentContext())
        {
            _tasks.Add(task);
        }

        TasksEmptyTextBlock.Visibility = _tasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TasksEmptyTextBlock.Text = GetTaskEmptyMessage();

        if (taskIdToSelect is null && selectFirstTask && _tasks.Count > 0)
        {
            taskIdToSelect = _tasks[0].Id;
        }

        if (taskIdToSelect is null)
        {
            ResetTaskForm();
            return;
        }

        WorkTask? taskToSelect = _tasks.FirstOrDefault(task => task.Id == taskIdToSelect.Value);

        if (taskToSelect is not null)
        {
            SelectTask(taskToSelect);
            return;
        }

        ResetTaskForm();
    }

    private void SelectTask(WorkTask task)
    {
        _selectedTask = task;
        TasksDataGrid.SelectedItem = task;
        TasksDataGrid.ScrollIntoView(task);
        TaskTitleTextBox.Text = task.Title;
        SelectTaskStatus(task.Status);
        SelectTaskPriority(task.Priority);
        SelectAssignedEmployee(task.AssignedEmployeeId);
        TaskDueDatePicker.SelectedDate = task.DueDate;
        UpdateTaskActionState();
    }

    private void LoadDocumentsForCurrentContext(int? documentIdToSelect = null, bool selectFirstDocument = false)
    {
        _documents.Clear();
        _selectedDocument = null;
        DocumentsDataGrid.SelectedItem = null;
        UpdateDocumentActionState();

        IEnumerable<ProjectDocument> documents;

        if (IsStaffUser())
        {
            if (_currentUser?.EmployeeId is null)
            {
                SelectedProjectForDocumentTextBlock.Text = "Bu kullanici bir employee kaydina bagli degil.";
                DocumentsEmptyTextBlock.Text = "Employee baglantisi olmadigi icin belge listelenemiyor.";
                DocumentsEmptyTextBlock.Visibility = Visibility.Visible;
                ClearDocumentForm();
                return;
            }

            SelectedProjectForDocumentTextBlock.Text = $"{_currentUser.FullName} icin atanmis task projelerindeki belgeler.";
            documents = _documentService.GetDocumentsForEmployeeProjects(_currentUser.EmployeeId.Value);
        }
        else
        {
            if (_selectedProject is null)
            {
                SelectedProjectForDocumentTextBlock.Text = "Once Projects sekmesinden bir project sec.";
                DocumentsEmptyTextBlock.Visibility = Visibility.Collapsed;
                ClearDocumentForm();
                return;
            }

            SelectedProjectForDocumentTextBlock.Text = $"Selected project: {_selectedProject.Name}";
            documents = _documentService.GetDocumentsForProject(_selectedProject.Id);
        }

        foreach (ProjectDocument document in documents)
        {
            _documents.Add(document);
        }

        DocumentsEmptyTextBlock.Text = IsStaffUser()
            ? "Erisebilecegin project belgeleri yok."
            : "Bu project icin henuz belge yok.";
        DocumentsEmptyTextBlock.Visibility = _documents.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        if (documentIdToSelect is null && selectFirstDocument && _documents.Count > 0)
        {
            documentIdToSelect = _documents[0].Id;
        }

        if (documentIdToSelect is null)
        {
            ClearDocumentForm();
            return;
        }

        ProjectDocument? documentToSelect = _documents.FirstOrDefault(document => document.Id == documentIdToSelect.Value);

        if (documentToSelect is not null)
        {
            SelectDocument(documentToSelect);
            return;
        }

        ClearDocumentForm();
    }

    private void SelectDocument(ProjectDocument document)
    {
        _selectedDocument = document;
        DocumentsDataGrid.SelectedItem = document;
        DocumentsDataGrid.ScrollIntoView(document);
        DocumentFileNameTextBox.Text = document.FileName;
        DocumentFilePathTextBox.Text = document.FilePath;
        SelectDocumentStatus(document.Status);
        DocumentNotesTextBox.Text = document.Notes;
        PopulateDocumentAIFields(document);
        UpdateDocumentActionState();
    }

    private void LoadEmployees(int? employeeIdToSelect = null)
    {
        _employees.Clear();
        _selectedEmployee = null;
        EmployeesDataGrid.SelectedItem = null;
        UpdateEmployeeActionState();

        foreach (Employee employee in _employeeService.GetEmployees())
        {
            _employees.Add(employee);
        }

        if (employeeIdToSelect is null)
        {
            ClearEmployeeForm();
            return;
        }

        Employee? employeeToSelect = _employees.FirstOrDefault(employee => employee.Id == employeeIdToSelect.Value);

        if (employeeToSelect is not null)
        {
            SelectEmployee(employeeToSelect);
        }
    }

    private void SelectEmployee(Employee employee)
    {
        _selectedEmployee = employee;
        EmployeesDataGrid.SelectedItem = employee;
        EmployeesDataGrid.ScrollIntoView(employee);
        EmployeeFullNameTextBox.Text = employee.FullName;
        EmployeeEmailTextBox.Text = employee.Email;
        EmployeeDepartmentTextBox.Text = employee.Department;
        EmployeeRoleTextBox.Text = employee.RoleTitle;
        SelectEmployeeAvailability(employee.AvailabilityStatus);
        EmployeeLeaveStartDatePicker.SelectedDate = employee.LeaveStart;
        EmployeeLeaveEndDatePicker.SelectedDate = employee.LeaveEnd;
        EmployeeSkillsTextBox.Text = employee.Skills;
        EmployeeBackupTextBox.Text = employee.BackupEmployeeName;
        EmployeeWorkloadSummaryTextBlock.Text = BuildEmployeeWorkloadSummary(employee);
        UpdateEmployeeActionState();
    }

    private void LoadUserAccounts(int? userIdToSelect = null)
    {
        _userAccounts.Clear();
        _selectedUserAccount = null;
        UserAccountsDataGrid.SelectedItem = null;
        UpdateUserAccountActionState();

        foreach (UserAccount user in _userAccountService.GetUserAccounts())
        {
            _userAccounts.Add(user);
        }

        if (userIdToSelect is null)
        {
            ClearUserForm();
            return;
        }

        UserAccount? userToSelect = _userAccounts.FirstOrDefault(user => user.Id == userIdToSelect.Value);

        if (userToSelect is not null)
        {
            SelectUserAccount(userToSelect);
        }
    }

    private void SelectUserAccount(UserAccount user)
    {
        _selectedUserAccount = user;
        UserAccountsDataGrid.SelectedItem = user;
        UserAccountsDataGrid.ScrollIntoView(user);
        UserEmailTextBox.Text = user.Email;
        SelectUserRole(user.Role);
        SelectUserEmployee(user.EmployeeId);
        UserIsActiveCheckBox.IsChecked = user.IsActive;
        NewUserPasswordBox.Clear();
        UpdateUserAccountActionState();
    }

    private void RefreshProjectOverview()
    {
        _allProjects.Clear();
        _dueSoonProjects.Clear();

        foreach (WorkProject project in _projectService.GetAllProjects())
        {
            _allProjects.Add(project);
        }

        foreach (WorkProject project in _projectService.GetProjectsDueWithinDays(7))
        {
            _dueSoonProjects.Add(project);
        }

        DueSoonNotificationTextBlock.Text = _dueSoonProjects.Count == 0
            ? "Son 7 gun icinde teslimi olan proje yok."
            : $"{_dueSoonProjects.Count} project icin teslim tarihi 7 gun icinde.";

        DueSoonNotificationTextBlock.Foreground = _dueSoonProjects.Count == 0
            ? System.Windows.Media.Brushes.SlateGray
            : System.Windows.Media.Brushes.Firebrick;
    }

    private void UpdateProjectActionState()
    {
        bool canUpdateProject = _currentUser?.HasPermission(PermissionNames.ProjectUpdate) == true
            && _selectedProject is not null;

        UpdateProjectStatusButton.IsEnabled = canUpdateProject;
    }

    private void UpdateTaskActionState()
    {
        bool canUpdateTask = _currentUser?.HasPermission(PermissionNames.TaskUpdate) == true
            && _selectedTask is not null;

        UpdateTaskButton.IsEnabled = canUpdateTask;
        MarkTaskDoneButton.IsEnabled = canUpdateTask && _selectedTask?.Status != TaskStatusNames.Done;
    }

    private void UpdateDocumentActionState()
    {
        bool canCreateDocument = _currentUser?.HasPermission(PermissionNames.DocumentCreate) == true;
        bool canUpdateDocument = _currentUser?.HasPermission(PermissionNames.DocumentUpdate) == true
            && _selectedDocument is not null;

        AddDocumentButton.IsEnabled = canCreateDocument && !IsStaffUser();
        UpdateDocumentStatusButton.IsEnabled = canUpdateDocument && !IsStaffUser();
        AnalyzeDocumentButton.IsEnabled = canUpdateDocument && !IsStaffUser();
    }

    private void UpdateEmployeeActionState()
    {
        bool canManageEmployees = _currentUser?.HasPermission(PermissionNames.EmployeeManage) == true;
        AddEmployeeButton.IsEnabled = canManageEmployees;
        UpdateEmployeeButton.IsEnabled = canManageEmployees && _selectedEmployee is not null;
    }

    private void UpdateUserAccountActionState()
    {
        bool canManageUsers = _currentUser?.HasPermission(PermissionNames.UserManage) == true;
        AddUserButton.IsEnabled = canManageUsers;
        UpdateUserButton.IsEnabled = canManageUsers && _selectedUserAccount is not null;
        ResetUserPasswordButton.IsEnabled = canManageUsers && _selectedUserAccount is not null;
    }

    private bool IsStaffUser()
    {
        return _currentUser?.Role == RoleNames.Staff;
    }

    private List<WorkTask> GetFilteredTasksForCurrentContext()
    {
        if (IsStaffUser())
        {
            if (_currentUser?.EmployeeId is null)
            {
                return [];
            }

            return ApplyTaskFilters(_taskService.GetTasksForEmployee(_currentUser.EmployeeId.Value)).ToList();
        }

        if (_selectedProject is null)
        {
            return [];
        }

        return ApplyTaskFilters(_taskService.GetTasksForProject(_selectedProject.Id)).ToList();
    }

    private IEnumerable<WorkTask> ApplyTaskFilters(IEnumerable<WorkTask> tasks)
    {
        IEnumerable<WorkTask> query = tasks;
        string statusFilter = GetSelectedComboBoxText(TaskStatusFilterComboBox);
        string priorityFilter = GetSelectedComboBoxText(TaskPriorityFilterComboBox);
        string dueDateFilter = GetSelectedComboBoxText(TaskDueDateFilterComboBox);

        if (statusFilter != "All statuses")
        {
            query = query.Where(task => task.Status == statusFilter);
        }

        if (priorityFilter != "All priorities")
        {
            query = query.Where(task => task.Priority == priorityFilter);
        }

        query = dueDateFilter switch
        {
            "Overdue" => query.Where(task => task.IsOverdue),
            "Due today" => query.Where(task => task.DueDate.HasValue
                && task.DueDate.Value.Date == DateTime.Today),
            "Due in 7 days" => query.Where(task => task.DueDate.HasValue
                && task.DueDate.Value.Date >= DateTime.Today
                && task.DueDate.Value.Date <= DateTime.Today.AddDays(7)),
            "No due date" => query.Where(task => !task.DueDate.HasValue),
            _ => query
        };

        return query;
    }

    private bool TaskIsVisibleInCurrentFilters(WorkTask task)
    {
        return GetFilteredTasksForCurrentContext().Any(filteredTask => filteredTask.Id == task.Id);
    }

    private string GetTaskEmptyMessage()
    {
        if (IsStaffUser())
        {
            bool hasActiveStaffFilter = GetSelectedComboBoxText(TaskStatusFilterComboBox) != "All statuses"
                || GetSelectedComboBoxText(TaskPriorityFilterComboBox) != "All priorities"
                || GetSelectedComboBoxText(TaskDueDateFilterComboBox) != "All dates";

            return hasActiveStaffFilter
                ? "Bu filtrelere uygun sana atanmis task bulunamadi."
                : "Sana atanmis acik veya kapali task yok.";
        }

        if (_selectedProject is null)
        {
            return "Once Projects sekmesinden bir project sec.";
        }

        bool hasActiveFilter = GetSelectedComboBoxText(TaskStatusFilterComboBox) != "All statuses"
            || GetSelectedComboBoxText(TaskPriorityFilterComboBox) != "All priorities"
            || GetSelectedComboBoxText(TaskDueDateFilterComboBox) != "All dates";

        return hasActiveFilter
            ? "Bu filtrelere uygun task bulunamadi."
            : "Bu project icin henuz task yok.";
    }

    private void ResetTaskForm()
    {
        _selectedTask = null;
        TaskTitleTextBox.Clear();
        SelectTaskStatus(TaskStatusNames.ToDo);
        SelectTaskPriority(TaskPriorityNames.Normal);
        SelectAssignedEmployee(null);
        TaskDueDatePicker.SelectedDate = DateTime.Today.AddDays(7);
        UpdateTaskActionState();
    }

    private static string GetSelectedComboBoxText(System.Windows.Controls.ComboBox comboBox)
    {
        if (comboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item
            && item.Content is string text)
        {
            return text;
        }

        return string.Empty;
    }

    private int? GetSelectedAssignedEmployeeId()
    {
        if (TaskAssignedEmployeeComboBox.SelectedValue is int employeeId)
        {
            return employeeId;
        }

        return null;
    }

    private void SelectAssignedEmployee(int? employeeId)
    {
        TaskAssignedEmployeeComboBox.SelectedValue = employeeId;

        if (employeeId is null)
        {
            TaskAssignedEmployeeComboBox.SelectedIndex = -1;
        }
    }

    private Employee? FindEmployee(int? employeeId)
    {
        if (employeeId is null)
        {
            return null;
        }

        return _employees.FirstOrDefault(employee => employee.Id == employeeId.Value);
    }

    private string GetEmployeeName(int? employeeId)
    {
        return FindEmployee(employeeId)?.FullName ?? "Unassigned";
    }

    private static string AppendAssignmentWarning(string message, Employee? employee)
    {
        if (employee is null || employee.AvailabilityStatus == EmployeeAvailabilityNames.Available)
        {
            return message;
        }

        string backupText = string.IsNullOrWhiteSpace(employee.BackupEmployeeName)
            ? "Yedek calisan tanimli degil."
            : $"Yedek: {employee.BackupEmployeeName}.";

        return $"{message} Uyari: {employee.FullName} su an {employee.AvailabilityStatus}. {backupText}";
    }

    private string GetSelectedProjectStatus()
    {
        if (ProjectStatusComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem
            && selectedItem.Content is string status)
        {
            return status;
        }

        return ProjectStatusNames.Planning;
    }

    private void SelectProjectStatus(string status)
    {
        foreach (object item in ProjectStatusComboBox.Items)
        {
            if (item is System.Windows.Controls.ComboBoxItem comboBoxItem
                && comboBoxItem.Content is string itemStatus
                && itemStatus == status)
            {
                ProjectStatusComboBox.SelectedItem = comboBoxItem;
                return;
            }
        }

        ProjectStatusComboBox.SelectedIndex = 0;
    }

    private void ShowProjectFormMessage(string message, bool isError)
    {
        ProjectFormMessageTextBlock.Text = message;
        ProjectFormMessageTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.SeaGreen;
        ProjectFormMessageTextBlock.Visibility = Visibility.Visible;
    }

    private string GetSelectedTaskStatus()
    {
        if (TaskStatusComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item
            && item.Content is string status)
        {
            return status;
        }

        return TaskStatusNames.ToDo;
    }

    private void SelectTaskStatus(string status)
    {
        foreach (object item in TaskStatusComboBox.Items)
        {
            if (item is System.Windows.Controls.ComboBoxItem comboBoxItem
                && comboBoxItem.Content?.ToString() == status)
            {
                TaskStatusComboBox.SelectedItem = comboBoxItem;
                return;
            }
        }

        TaskStatusComboBox.SelectedIndex = 0;
    }

    private string GetSelectedTaskPriority()
    {
        if (TaskPriorityComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item
            && item.Content is string priority)
        {
            return priority;
        }

        return TaskPriorityNames.Normal;
    }

    private void SelectTaskPriority(string priority)
    {
        foreach (object item in TaskPriorityComboBox.Items)
        {
            if (item is System.Windows.Controls.ComboBoxItem comboBoxItem
                && comboBoxItem.Content?.ToString() == priority)
            {
                TaskPriorityComboBox.SelectedItem = comboBoxItem;
                return;
            }
        }

        TaskPriorityComboBox.SelectedIndex = 1;
    }

    private void ShowTaskFormMessage(string message, bool isError)
    {
        TaskFormMessageTextBlock.Text = message;
        TaskFormMessageTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.SeaGreen;
        TaskFormMessageTextBlock.Visibility = Visibility.Visible;
    }

    private bool TryReadEmployeeForm(
        out string fullName,
        out string email,
        out string department,
        out string roleTitle,
        out string availability,
        out DateTime? leaveStart,
        out DateTime? leaveEnd,
        out string skills,
        out string backupEmployeeName)
    {
        fullName = EmployeeFullNameTextBox.Text.Trim();
        email = EmployeeEmailTextBox.Text.Trim();
        department = EmployeeDepartmentTextBox.Text.Trim();
        roleTitle = EmployeeRoleTextBox.Text.Trim();
        availability = GetSelectedEmployeeAvailability();
        leaveStart = EmployeeLeaveStartDatePicker.SelectedDate;
        leaveEnd = EmployeeLeaveEndDatePicker.SelectedDate;
        skills = EmployeeSkillsTextBox.Text.Trim();
        backupEmployeeName = EmployeeBackupTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(department)
            || string.IsNullOrWhiteSpace(roleTitle))
        {
            ShowEmployeeFormMessage("Ad soyad, email, departman ve rol alanlari zorunludur.", isError: true);
            return false;
        }

        if (leaveStart.HasValue && leaveEnd.HasValue && leaveStart.Value.Date > leaveEnd.Value.Date)
        {
            ShowEmployeeFormMessage("Izin baslangic tarihi bitis tarihinden sonra olamaz.", isError: true);
            return false;
        }

        return true;
    }

    private void ClearEmployeeForm()
    {
        _selectedEmployee = null;
        EmployeesDataGrid.SelectedItem = null;
        EmployeeFullNameTextBox.Clear();
        EmployeeEmailTextBox.Clear();
        EmployeeDepartmentTextBox.Clear();
        EmployeeRoleTextBox.Clear();
        SelectEmployeeAvailability(EmployeeAvailabilityNames.Available);
        EmployeeLeaveStartDatePicker.SelectedDate = null;
        EmployeeLeaveEndDatePicker.SelectedDate = null;
        EmployeeSkillsTextBox.Clear();
        EmployeeBackupTextBox.Clear();
        EmployeeWorkloadSummaryTextBlock.Text = "Listeden bir calisan sec.";
        EmployeeFormMessageTextBlock.Visibility = Visibility.Collapsed;
        UpdateEmployeeActionState();
    }

    private string GetSelectedEmployeeAvailability()
    {
        if (EmployeeAvailabilityComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item
            && item.Content is string availability)
        {
            return availability;
        }

        return EmployeeAvailabilityNames.Available;
    }

    private void SelectEmployeeAvailability(string availability)
    {
        foreach (object item in EmployeeAvailabilityComboBox.Items)
        {
            if (item is System.Windows.Controls.ComboBoxItem comboBoxItem
                && comboBoxItem.Content?.ToString() == availability)
            {
                EmployeeAvailabilityComboBox.SelectedItem = comboBoxItem;
                return;
            }
        }

        EmployeeAvailabilityComboBox.SelectedIndex = 0;
    }

    private void ShowEmployeeFormMessage(string message, bool isError)
    {
        EmployeeFormMessageTextBlock.Text = message;
        EmployeeFormMessageTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.SeaGreen;
        EmployeeFormMessageTextBlock.Visibility = Visibility.Visible;
    }

    private static string BuildEmployeeWorkloadSummary(Employee employee)
    {
        string nextDueText = employee.NextOpenTaskDueDate.HasValue
            ? employee.NextOpenTaskDueDate.Value.ToString("dd.MM.yyyy")
            : "yaklasan teslim yok";

        string coverageText = employee.NeedsCoverage
            ? $" Kapsama gerekli; yedek kisi: {employee.BackupEmployeeName}."
            : string.Empty;

        return $"{employee.OpenTaskCount} acik task var. En yakin teslim: {nextDueText}.{coverageText}";
    }

    private bool TryReadUserForm(
        bool requirePassword,
        out string email,
        out string password,
        out string role,
        out int? employeeId,
        out bool isActive)
    {
        email = UserEmailTextBox.Text.Trim().ToLowerInvariant();
        password = NewUserPasswordBox.Password.Trim();
        role = GetSelectedUserRole();
        employeeId = GetSelectedUserEmployeeId();
        isActive = UserIsActiveCheckBox.IsChecked == true;

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            ShowUserFormMessage("Gecerli bir email zorunludur.", isError: true);
            return false;
        }

        if (requirePassword && password.Length < 6)
        {
            ShowUserFormMessage("Yeni kullanici sifresi en az 6 karakter olmalidir.", isError: true);
            return false;
        }

        if (role is not RoleNames.Admin and not RoleNames.Manager and not RoleNames.Staff)
        {
            ShowUserFormMessage("Role secimi zorunludur.", isError: true);
            return false;
        }

        return true;
    }

    private void ClearUserForm()
    {
        _selectedUserAccount = null;
        UserAccountsDataGrid.SelectedItem = null;
        UserEmailTextBox.Clear();
        NewUserPasswordBox.Clear();
        SelectUserRole(RoleNames.Staff);
        SelectUserEmployee(null);
        UserIsActiveCheckBox.IsChecked = true;
        UserFormMessageTextBlock.Visibility = Visibility.Collapsed;
        UpdateUserAccountActionState();
    }

    private string GetSelectedUserRole()
    {
        if (UserRoleComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item
            && item.Content is string role)
        {
            return role;
        }

        return RoleNames.Staff;
    }

    private void SelectUserRole(string role)
    {
        foreach (object item in UserRoleComboBox.Items)
        {
            if (item is System.Windows.Controls.ComboBoxItem comboBoxItem
                && comboBoxItem.Content?.ToString() == role)
            {
                UserRoleComboBox.SelectedItem = comboBoxItem;
                return;
            }
        }

        UserRoleComboBox.SelectedIndex = 2;
    }

    private int? GetSelectedUserEmployeeId()
    {
        if (UserEmployeeComboBox.SelectedValue is int employeeId)
        {
            return employeeId;
        }

        return null;
    }

    private void SelectUserEmployee(int? employeeId)
    {
        UserEmployeeComboBox.SelectedValue = employeeId;

        if (employeeId is null)
        {
            UserEmployeeComboBox.SelectedIndex = -1;
        }
    }

    private void ShowUserFormMessage(string message, bool isError)
    {
        UserFormMessageTextBlock.Text = message;
        UserFormMessageTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.SeaGreen;
        UserFormMessageTextBlock.Visibility = Visibility.Visible;
    }

    private bool TryReadDocumentForm(out string fileName, out string filePath, out string status, out string notes)
    {
        fileName = DocumentFileNameTextBox.Text.Trim();
        filePath = DocumentFilePathTextBox.Text.Trim();
        status = GetSelectedDocumentStatus();
        notes = DocumentNotesTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(filePath))
        {
            ShowDocumentFormMessage("Belge adi ve dosya yolu zorunludur.", isError: true);
            return false;
        }

        return true;
    }

    private void ClearDocumentForm()
    {
        _selectedDocument = null;
        DocumentsDataGrid.SelectedItem = null;
        DocumentFileNameTextBox.Clear();
        DocumentFilePathTextBox.Clear();
        SelectDocumentStatus(DocumentStatusNames.Uploaded);
        DocumentNotesTextBox.Clear();
        DocumentAIStatusTextBlock.Text = AIAnalysisStatusNames.NotAnalyzed;
        DocumentAISummaryTextBox.Clear();
        DocumentAIRiskNotesTextBox.Clear();
        DocumentFormMessageTextBlock.Visibility = Visibility.Collapsed;
        UpdateDocumentActionState();
    }

    private void PopulateDocumentAIFields(ProjectDocument document)
    {
        string analyzedText = document.AnalyzedAt.HasValue
            ? $"Analyzed at {document.AnalyzedAt.Value:dd.MM.yyyy HH:mm}"
            : "Not analyzed yet";

        DocumentAIStatusTextBlock.Text = $"{document.AIAnalysisStatus} | {analyzedText}";
        DocumentAISummaryTextBox.Text = document.AISummary;
        DocumentAIRiskNotesTextBox.Text = document.AIRiskNotes;
    }

    private string GetSelectedDocumentStatus()
    {
        if (DocumentStatusComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item
            && item.Content is string status)
        {
            return status;
        }

        return DocumentStatusNames.Uploaded;
    }

    private void SelectDocumentStatus(string status)
    {
        foreach (object item in DocumentStatusComboBox.Items)
        {
            if (item is System.Windows.Controls.ComboBoxItem comboBoxItem
                && comboBoxItem.Content?.ToString() == status)
            {
                DocumentStatusComboBox.SelectedItem = comboBoxItem;
                return;
            }
        }

        DocumentStatusComboBox.SelectedIndex = 0;
    }

    private void ShowDocumentFormMessage(string message, bool isError)
    {
        DocumentFormMessageTextBlock.Text = message;
        DocumentFormMessageTextBlock.Foreground = isError
            ? System.Windows.Media.Brushes.Firebrick
            : System.Windows.Media.Brushes.SeaGreen;
        DocumentFormMessageTextBlock.Visibility = Visibility.Visible;
    }
}
