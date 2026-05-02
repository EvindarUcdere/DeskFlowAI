using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DeskFlowAI.Models;
using DeskFlowAI.Services;

namespace DeskFlowAI;

public partial class MainWindow : Window
{
    private readonly DemoAuthService _authService = new();
    private readonly DemoDashboardService _dashboardService = new();
    private readonly DemoCustomerService _customerService = new();
    private readonly ObservableCollection<Customer> _customers = [];
    private readonly ObservableCollection<Customer> _filteredCustomers = [];
    private Customer? _selectedCustomer;

    public MainWindow()
    {
        InitializeComponent();
        LoadCustomers();
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
        DashboardSummary summary = _dashboardService.GetSummaryFor(user);

        LoginErrorTextBlock.Visibility = Visibility.Collapsed;
        SignedInUserTextBlock.Text = $"{user.FullName} | {user.Role} | {user.Email}";
        ActiveProjectsTextBlock.Text = summary.ActiveProjects.ToString();
        OpenTasksTextBlock.Text = summary.OpenTasks.ToString();
        OverdueTasksTextBlock.Text = summary.OverdueTasks.ToString();
        PendingAiDocumentsTextBlock.Text = summary.PendingAiDocuments.ToString();
        LoginView.Visibility = Visibility.Collapsed;
        DashboardView.Visibility = Visibility.Visible;
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

    private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadCustomerForm(out string companyName, out string contactName, out string email))
        {
            return;
        }

        Customer customer = _customerService.CreateCustomer(companyName, contactName, email);
        _customers.Add(customer);
        ApplyCustomerFilter();
        CustomersDataGrid.SelectedItem = customer;
        ClearCustomerForm();
        ShowCustomerFormMessage("Customer eklendi.", isError: false);
    }

    private void UpdateCustomerButton_Click(object sender, RoutedEventArgs e)
    {
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
        int selectedIndex = _customers.IndexOf(_selectedCustomer);

        if (selectedIndex >= 0)
        {
            _customers[selectedIndex] = updatedCustomer;
            ApplyCustomerFilter();
            CustomersDataGrid.SelectedItem = updatedCustomer;
            _selectedCustomer = updatedCustomer;
            ShowCustomerFormMessage("Selected customer guncellendi.", isError: false);
        }
    }

    private void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        if (CustomersDataGrid.SelectedItem is not Customer selectedCustomer)
        {
            ShowCustomerFormMessage("Silmek icin listeden bir customer sec.", isError: true);
            return;
        }

        _customers.Remove(selectedCustomer);
        ApplyCustomerFilter();
        ClearCustomerForm();
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
    }

    private void ClearCustomerFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearCustomerForm();
    }

    private void ClearCustomerForm()
    {
        _selectedCustomer = null;
        CustomersDataGrid.SelectedItem = null;
        CustomerFormTitleTextBlock.Text = "Add customer";
        CustomerCompanyTextBox.Clear();
        CustomerContactTextBox.Clear();
        CustomerEmailTextBox.Clear();
        CustomerFormMessageTextBlock.Visibility = Visibility.Collapsed;
        CustomerCompanyTextBox.Focus();
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
}
