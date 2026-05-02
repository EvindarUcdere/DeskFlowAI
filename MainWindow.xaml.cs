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
    private readonly DemoAuditLogService _auditLogService = new();
    private readonly ObservableCollection<Customer> _customers = [];
    private readonly ObservableCollection<Customer> _filteredCustomers = [];
    private readonly ObservableCollection<WorkProject> _projects = [];
    private readonly ObservableCollection<WorkTask> _tasks = [];
    private readonly ObservableCollection<WorkProject> _allProjects = [];
    private readonly ObservableCollection<WorkProject> _dueSoonProjects = [];
    private readonly ObservableCollection<AuditLogEntry> _auditLogs = [];
    private Customer? _selectedCustomer;
    private WorkProject? _selectedProject;
    private WorkTask? _selectedTask;
    private UserSession? _currentUser;

    public MainWindow()
    {
        InitializeComponent();
        new DatabaseInitializer().Initialize();
        LoadCustomers();
        LoadAuditLogs();
        ProjectsDataGrid.ItemsSource = _projects;
        TasksDataGrid.ItemsSource = _tasks;
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
        TaskDueDatePicker.SelectedDate = selectedTask.DueDate;
        TaskFormMessageTextBlock.Visibility = Visibility.Collapsed;
        UpdateTaskActionState();
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
        CustomersDataGrid.SelectedItem = null;
        ProjectsDataGrid.SelectedItem = null;
        TasksDataGrid.SelectedItem = null;
        CustomerFormTitleTextBlock.Text = "Add customer";
        CustomerCompanyTextBox.Clear();
        CustomerContactTextBox.Clear();
        CustomerEmailTextBox.Clear();
        CustomerFormMessageTextBlock.Visibility = Visibility.Collapsed;
        ProjectNameTextBox.Clear();
        ProjectDueDatePicker.SelectedDate = DateTime.Today.AddDays(14);
        ProjectFormMessageTextBlock.Visibility = Visibility.Collapsed;
        TaskTitleTextBox.Clear();
        TaskDueDatePicker.SelectedDate = DateTime.Today.AddDays(7);
        TaskFormMessageTextBlock.Visibility = Visibility.Collapsed;
        _tasks.Clear();
        SelectedProjectForTaskTextBlock.Text = "Once Projects sekmesinden bir project sec.";
        TasksEmptyTextBlock.Visibility = Visibility.Collapsed;
        LoadProjectsForSelectedCustomer();
        UpdateProjectActionState();
        UpdateTaskActionState();
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
        DateTime? dueDate = TaskDueDatePicker.SelectedDate;

        if (string.IsNullOrWhiteSpace(title))
        {
            ShowTaskFormMessage("Task title zorunludur.", isError: true);
            return;
        }

        WorkTask task = _taskService.CreateTask(_selectedProject.Id, title, status, priority, dueDate);
        LoadTasksForSelectedProject(task.Id);
        RefreshDashboardSummary();
        string dueDateText = dueDate.HasValue ? dueDate.Value.ToString("dd.MM.yyyy") : "teslim tarihi yok";
        RecordAudit("Created", "Task", $"{task.Title} gorevi {_selectedProject.Name} projesine {priority} priority ile eklendi. Due: {dueDateText}.");
        ShowTaskFormMessage("Task eklendi ve listede secildi.", isError: false);
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
        DateTime? newDueDate = TaskDueDatePicker.SelectedDate;

        string oldStatus = _selectedTask.Status;
        string oldPriority = _selectedTask.Priority;
        DateTime? oldDueDate = _selectedTask.DueDate;

        WorkTask updatedTask = _taskService.UpdateTaskWorkflow(_selectedTask, newStatus, newPriority, newDueDate);
        LoadTasksForSelectedProject(updatedTask.Id);
        RefreshDashboardSummary();

        string oldDueDateText = oldDueDate.HasValue ? oldDueDate.Value.ToString("dd.MM.yyyy") : "none";
        string newDueDateText = newDueDate.HasValue ? newDueDate.Value.ToString("dd.MM.yyyy") : "none";
        RecordAudit("Updated", "Task", $"{updatedTask.Title}: Status '{oldStatus}' -> '{newStatus}'; Priority '{oldPriority}' -> '{newPriority}'; Due '{oldDueDateText}' -> '{newDueDateText}'");
        ShowTaskFormMessage("Task guncellendi.", isError: false);
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
        AddTaskButton.IsEnabled = user.HasPermission(PermissionNames.TaskCreate);
        UpdateTaskActionState();
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

    private void LoadProjectsForSelectedCustomer(int? projectIdToSelect = null, bool selectFirstProject = false)
    {
        _projects.Clear();
        _selectedProject = null;
        _selectedTask = null;
        ProjectsDataGrid.SelectedItem = null;
        TasksDataGrid.SelectedItem = null;
        _tasks.Clear();
        SelectedProjectForTaskTextBlock.Text = "Once Projects sekmesinden bir project sec.";
        TasksEmptyTextBlock.Visibility = Visibility.Collapsed;
        UpdateProjectActionState();
        UpdateTaskActionState();

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
        UpdateProjectActionState();
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

        foreach (WorkTask task in _taskService.GetTasksForProject(_selectedProject.Id))
        {
            _tasks.Add(task);
        }

        TasksEmptyTextBlock.Visibility = _tasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        if (taskIdToSelect is null && selectFirstTask && _tasks.Count > 0)
        {
            taskIdToSelect = _tasks[0].Id;
        }

        if (taskIdToSelect is null)
        {
            TaskTitleTextBox.Clear();
            SelectTaskStatus(TaskStatusNames.ToDo);
            SelectTaskPriority(TaskPriorityNames.Normal);
            TaskDueDatePicker.SelectedDate = DateTime.Today.AddDays(7);
            return;
        }

        WorkTask? taskToSelect = _tasks.FirstOrDefault(task => task.Id == taskIdToSelect.Value);

        if (taskToSelect is not null)
        {
            SelectTask(taskToSelect);
        }
    }

    private void SelectTask(WorkTask task)
    {
        _selectedTask = task;
        TasksDataGrid.SelectedItem = task;
        TasksDataGrid.ScrollIntoView(task);
        TaskTitleTextBox.Text = task.Title;
        SelectTaskStatus(task.Status);
        SelectTaskPriority(task.Priority);
        TaskDueDatePicker.SelectedDate = task.DueDate;
        UpdateTaskActionState();
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
}
