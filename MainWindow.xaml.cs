using System.Windows;
using System.Windows.Input;

namespace DeskFlowAI;

public partial class MainWindow : Window
{
    private const string DemoEmail = "admin@deskflow.ai";
    private const string DemoPassword = "Admin123";

    public MainWindow()
    {
        InitializeComponent();
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

        if (email.Equals(DemoEmail, StringComparison.OrdinalIgnoreCase) && password == DemoPassword)
        {
            ShowDashboard(email);
            return;
        }

        LoginErrorTextBlock.Text = "Email veya sifre hatali. Demo kullanici: admin@deskflow.ai / Admin123";
        LoginErrorTextBlock.Visibility = Visibility.Visible;
    }

    private void ShowDashboard(string email)
    {
        LoginErrorTextBlock.Visibility = Visibility.Collapsed;
        SignedInUserTextBlock.Text = $"Signed in as {email}";
        LoginView.Visibility = Visibility.Collapsed;
        DashboardView.Visibility = Visibility.Visible;
    }
}
